using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;

namespace CryptoExchangeLib.Common
{
    public class Bittrex : IExchange
    {
        private HttpClient _client;
        private HttpClient _privateClient;

        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string BaseUrl { get; set; }

        public Bittrex(string apiBaseUrl = "https://bittrex.com")
        {
            BaseUrl = apiBaseUrl;
            _client = HttpClientFactory.Create();

        }

        public Bittrex(string apiKey, string apiSecret, string apiBaseUrl = "https://bittrex.com")
        {
            BaseUrl = apiBaseUrl;
            ApiKey = apiKey;
            ApiSecret = apiSecret;
            _client = HttpClientFactory.Create();
            InitializePrivate();
        }

        public void Dispose()
        {
            if (_client != null)
            {
                _client.Dispose();
            }

            if (_privateClient != null)
            {
                _privateClient.Dispose();
            }
        }

        private T GetObject<T>(string jsonData)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonData)))
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                object o = (object)serializer.ReadObject(stream);
                return (T)o;
            }
        }

        public void InitializePrivate()
        {
            _privateClient = HttpClientFactory.Create(new AuthDelegatingHandler(ApiKey, ApiSecret));
        }

        public void InitializePrivate(string key, string secret)
        {
            ApiKey = key;
            ApiSecret = secret;

            InitializePrivate();
        }

        #region Public API Implementation

        public T GetResult<T>(PublicApiCall call, string requestData)
            where T : IResponse, new()
        {
            var s = string.Format("{0}/api/v1.1/public/{1}", BaseUrl, requestData);

            System.Net.Http.Headers.HttpRequestHeaders h = _client.DefaultRequestHeaders;

            var response = _client.GetStringAsync(s);
            if (string.IsNullOrEmpty(response.Result))
            {
                return new T() { Success = false, Error = "No Response." };
            }

            string result = DoPublic(call, response.Result);

            return GetObject<T>(result);
        }

        public CurrenciesResponse GetCurrencies()
        {
            var query = "getcurrencies";
            return GetResult<CurrenciesResponse>(PublicApiCall.GetCurrencies, query);
        }

        public TradePairsResponse GetTradePairs()
        {
            var query = "getmarkets";
            return GetResult<TradePairsResponse>(PublicApiCall.GetTradePairs, query);
        }

        public MarketsResponse GetMarkets(MarketsRequest request)
        {
            var query = "getmarketsummaries";
            MarketsResponse r =  GetResult<MarketsResponse>(PublicApiCall.GetMarkets, query);

            foreach(var c in r.Data)
            {
                c.Change = ((c.LastPrice / c.PrevDay) - 1) * 100;
            }

            return r;
        }

        public MarketResponse GetMarket(MarketRequest request)
        {
            var query = "getmarketsummary?market=" + $"{request.TradePair.PairLabel}";
            return GetResult<MarketResponse>(PublicApiCall.GetMarket, query);
        }

        public MarketHistoryResponse GetMarketHistory(MarketHistoryRequest request)
        {
            var query = "getmarkethistory?market=" + $"{request.TradePair.PairLabel}";
            MarketHistoryResponse r = GetResult<MarketHistoryResponse>(PublicApiCall.GetMarketHistory, query);

            foreach(var c in r.Data)
            {
                c.Label = request.TradePair.PairLabel;
            }

            return r;

        }

        public MarketOrdersResponse GetMarketOrders(MarketOrdersRequest request)
        {
            var query = "getorderbook?market=" + $"{request.TradePair.PairLabel}" + "&type=both";
            MarketOrdersResponse r = GetResult<MarketOrdersResponse>(PublicApiCall.GetMarketOrders, query);

            foreach(var c in r.Data.Buy)
            {
                c.Label = request.TradePair.PairLabel;
                c.Total = c.Price * c.Volume;
            }

            foreach(var c in r.Data.Sell)
            {
                c.Label = request.TradePair.PairLabel;
                c.Total = c.Price * c.Volume;
            }

            return r;
        }


        #endregion


        #region Private API Implementation

        public T GetResult<T, U>(PrivateApiCall call, U requestData)
            where T : IResponse
            where U : IRequest
        {
            return (T)new object();
        }

        private string byteToString(byte[] buff)
        {
            string sbinary = "";
            for (int i = 0; i < buff.Length; i++)
                sbinary += buff[i].ToString("X2"); /* hex format */
            return sbinary;
        }

        public T GetResult<T>(PrivateApiCall call, string requestData)
            where T : IResponse
        {
            var nonce = DateTime.Now.Ticks.ToString();
            string requestContentBase64String = string.Empty;
            string jsonData = JsonConvert.SerializeObject(requestData);
            string url = string.Format("{0}/api/v1.1/account/{1}apikey=" + ApiKey + "&nonce=" + nonce, BaseUrl.TrimEnd('/'), requestData);
            HttpWebRequest http = (HttpWebRequest)WebRequest.Create(new Uri(url));
            http.Accept = "application/json";
            http.Method = "GET";

            var uriBytes = Encoding.UTF8.GetBytes(url);
            var apiSecretBytes = Encoding.UTF8.GetBytes(ApiSecret);
            using(var hmac = new HMACSHA512(apiSecretBytes))
            {
                var hash = hmac.ComputeHash(uriBytes);
                var hashText = byteToString(hash);
                http.Headers.Add("apisign: " + hashText);
            }

            var response = http.GetResponse();
            var stream = response.GetResponseStream();
            var sr = new StreamReader(stream);
            var content = sr.ReadToEnd();

            string result = DoPrivate(call, content);

            return JsonConvert.DeserializeObject<T>(result);
        }

        public BalanceResponse GetBalances(BalanceRequest request)
        {
            string query = "";

            if(request.Currency != null)
            {
                query = "getbalance?currency=" + request.Currency + "&";
            }
            else
            {
                query = "getbalances?";
            }

            return GetResult<BalanceResponse>(PrivateApiCall.GetBalance, query);
        }

        public CancelTradeResponse CancelTrade(CancelTradeRequest request)
        {
            throw new NotImplementedException();
        }

        public DepositAddressResponse GetDepositAddress(DepositAddressRequest request)
        {
            string query = "";

            if(request.Currency != null)
            {
                query = "getdepositaddress?currency=" + request.Currency + "&";
            }

            return GetResult<DepositAddressResponse>(PrivateApiCall.GetDepositAddress, query);
        }

        public OpenOrdersResponse GetOpenOrders(OpenOrdersRequest request)
        {
            throw new NotImplementedException();
        }

        public TradeHistoryResponse GetTradeHistory(TradeHistoryRequest request)
        {
            string query = "";

            if (request.Market != null)
            {
                query = "getorderhistory?market=" + request.Market + "&";
            }
            else
            {
                query = "getorderhistory?";
            }

            TradeHistoryResponse r = GetResult<TradeHistoryResponse>(PrivateApiCall.GetTradeHistory, query);

            foreach(var c in r.Data)
            {
                c.Total = (c.Amount * c.Rate) + c.Fee;
            }

            return r;
        }

        public TransactionResponse GetTransactions(TransactionRequest request)
        {
            string query = "";
            string query2 = "";

            if(request.Currency != null)
            {
                query = "getwithdrawalhistory?currency=" + request.Currency + "&";
                query2 = "getdeposithistory?currency=" + request.Currency + "&";
            } else
            {
                query = "getwithdrawalhistory?";
                query2 = "getdeposithistory?";
            }

            TransactionResponse r = GetResult<TransactionResponse>(PrivateApiCall.GetTransactions, query);
            TransactionResponse q = GetResult<TransactionResponse>(PrivateApiCall.GetTransactions, query2);

            foreach(var c in r.Data)
            {
                c.Type = "Withdrawl";
            }

            foreach(var c in q.Data)
            {
                c.Type = "Deposit";
                r.Data.Add(c);
            }

            return r;
        }

        public SubmitTradeResponse SubmitTrade(SubmitTradeRequest request)
        {
            throw new NotImplementedException();
        }

        public SubmitWithdrawResponse SubmitWithdraw(SubmitWithdrawRequest request)
        {
            throw new NotImplementedException();
        }

        #endregion


        #region Shim Implementation

        public string DoPublic(PublicApiCall call, string response)
        {
            string ret = response;

            ret = ret.Replace("\"result\":", "\"Data\":");
            ret = ret.Replace("\"success\":", "\"Success\":");
            ret = ret.Replace("\"message\":", "\"Message\":");

            switch (call)
            {
                case PublicApiCall.GetCurrencies:
                    ret = ret.Replace("CurrencyLong", "Name");
                    ret = ret.Replace("Currency", "Symbol");
                    ret = ret.Replace("MinConfirmation", "DepositConfirmations");
                    ret = ret.Replace("TxFee", "WithdrawFee");
                    ret = ret.Replace("\"IsActive\":true", "\"Status\":\"OK\"");
                    ret = ret.Replace("\"IsActive\":false", "\"Status\":\"Offline\"");
                    ret = ret.Replace("CoinType", "Algorithm");
                    ret = ret.Replace("Notice", "StatusMessage");

                    break;

                case PublicApiCall.GetTradePairs:
                    ret = ret.Replace("MarketCurrencyLong", "Currency");
                    ret = ret.Replace("MarketCurrency", "Symbol");
                    ret = ret.Replace("BaseCurrency", "BaseSymbol");
                    ret = ret.Replace("BaseSymbolLong", "BaseCurrency");
                    ret = ret.Replace("MinTradeSize", "MinimumBaseTrade");
                    ret = ret.Replace("MarketName", "Label");
                    ret = ret.Replace("\"IsActive\":true", "\"Status\":\"OK\"");
                    ret = ret.Replace("\"IsActive\":false", "\"Status\":\"Offline\"");

                    break;

                case PublicApiCall.GetMarket:
                    ret = ret.Replace("\"Data\":[{", "\"Data\":{");
                    ret = ret.Replace("}]}", "}}");
                    ret = ret.Replace("MarketName", "Label");
                    ret = ret.Replace("Last", "LastPrice");
                    ret = ret.Replace("Ask", "AskPrice");
                    ret = ret.Replace("Bid", "BidPrice");

                    break;

                case PublicApiCall.GetMarkets:
                    ret = ret.Replace("MarketName", "Label");
                    ret = ret.Replace("Last", "LastPrice");
                    ret = ret.Replace("Ask", "AskPrice");
                    ret = ret.Replace("Bid", "BidPrice");

                    break;

                case PublicApiCall.GetMarketHistory:
                    ret = ret.Replace("Quantity", "Amount");
                    ret = ret.Replace("OrderType", "Type");


                    break;

                case PublicApiCall.GetMarketOrders:
                    ret = ret.Replace("buy", "Buy");
                    ret = ret.Replace("sell", "Sell");
                    ret = ret.Replace("Quantity", "Volume");
                    ret = ret.Replace("Rate", "Price");


                    break;

            }

            return ret;
        }

        public string DoPrivate(PrivateApiCall call, string response)
        {
            string ret = response;

            ret = ret.Replace("\"result\":[", "\"Data\":[");
            ret = ret.Replace("\"success\":", "\"Success\":");
            ret = ret.Replace("\"message\":", "\"Message\":");



            switch (call)
            {
                case PrivateApiCall.GetBalance:
                    //encapsulate single data items in a list, since this is what the deserialized object holds.
                    if (ret.Contains("\"result\":{"))
                    {
                        ret = ret.Replace("\"result\":{", "\"Data\":[{");
                        ret = ret.Replace("}}", "}]}");
                    }

                    ret = ret.Replace("Currency", "Symbol");
                    ret = ret.Replace("Balance", "Total");
                    ret = ret.Replace("Pending", "Unconfirmed");
                    ret = ret.Replace("CryptoAddress", "Address");
                    ret = ret.Replace("UUID", "CurrencyId");
                    break;

                case PrivateApiCall.GetDepositAddress:
                    ret = ret.Replace("\"result\":", "\"Data\":");
                    break;

                case PrivateApiCall.GetTradeHistory:
                    if (ret.Contains("\"result\":{"))
                    {
                        ret = ret.Replace("\"result\":{", "\"Data\":[{");
                        ret = ret.Replace("}}", "}]}");
                        ret = ret.Replace("OrderUuid", "TradeId");
                        ret = ret.Replace("Exchange", "Market");
                        ret = ret.Replace("OrderType", "Type");
                        ret = ret.Replace("Quantity", "Amount");
                        ret = ret.Replace("Price", "Rate");
                        ret = ret.Replace("Commission", "Fee");
                    }
                    break;

                case PrivateApiCall.GetTransactions:
                    ret = ret.Replace("\"result\":", "\"Data\":");
                    ret = ret.Replace("PaymentUuid", "Id");
                    ret = ret.Replace("LastUpdated", "Timestamp");
                    ret = ret.Replace("Authorized", "Status");
                    ret = ret.Replace("TxCost", "Fee");
                    break;
            }

            return ret;
        }

        public string GetTradePairLabel(TradePair tradePair)
        {
            return tradePair.PairLabel;
        }

        #endregion


    }

}
