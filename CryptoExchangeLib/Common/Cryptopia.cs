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
    class Cryptopia : IExchange
    {
        private HttpClient _client;
        private HttpClient _privateClient;

        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string BaseUrl { get; set; }

        public Cryptopia(string apiBaseUrl = "https://www.cryptopia.co.nz")
        {
            BaseUrl = apiBaseUrl;
            _client = HttpClientFactory.Create();
            
        }

        public Cryptopia(string apiKey, string apiSecret, string apiBaseUrl = "https://www.cryptopia.co.nz")
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

            if(_privateClient != null)
            {
                _privateClient.Dispose();
            }
        }

        private T GetObject<T>(string jsonData)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonData)))
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                return (T)(object)serializer.ReadObject(stream);
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
            var s = string.Format("{0}/Api/{1}{2}", BaseUrl, call, requestData);

            System.Net.Http.Headers.HttpRequestHeaders h = _client.DefaultRequestHeaders;

            var response = _client.GetStringAsync(s);
            if (string.IsNullOrEmpty(response.Result))
            {
                return new T() {    Success = false, Error = "No Response." };
            }
            return GetObject<T>(response.Result);
        }

        public CurrenciesResponse GetCurrencies()
        {
            return GetResult<CurrenciesResponse>(PublicApiCall.GetCurrencies, null);
        }

        public TradePairsResponse GetTradePairs()
        {
            return GetResult<TradePairsResponse>(PublicApiCall.GetTradePairs, null);
        }

        public MarketsResponse GetMarkets(MarketsRequest request)
        {
            var query = request.Hours.HasValue ? $"/{request.Hours}" : null;
            return GetResult<MarketsResponse>(PublicApiCall.GetMarkets, query);
        }

        public MarketResponse GetMarket(MarketRequest request)
        {
            string tradePairId = request.TradePair.Id.HasValue ? request.TradePair.Id.ToString() : request.TradePair.CurrencyLabel + "_" + request.TradePair.BaseLabel;
            var query = request.Hours.HasValue ? $"/{tradePairId}/{request.Hours}" : $"/{tradePairId}";
            return GetResult<MarketResponse>(PublicApiCall.GetMarket, query);
        }

        public MarketHistoryResponse GetMarketHistory(MarketHistoryRequest request)
        {
            var query = $"/{GetTradePairLabel(request.TradePair)}";
            return GetResult<MarketHistoryResponse>(PublicApiCall.GetMarketHistory, query);
        }

        public MarketOrdersResponse GetMarketOrders(MarketOrdersRequest request)
        {
            string tradePairLabel = GetTradePairLabel(request.TradePair);
            var query = request.OrderCount.HasValue ? $"/{tradePairLabel}/{request.OrderCount}" : $"/{tradePairLabel}";
            return GetResult<MarketOrdersResponse>(PublicApiCall.GetMarketOrders, query);
        }

        #endregion

        #region Private API Implementation

        public async Task<T> GetResultOld<T, U>(PrivateApiCall call, U requestData)
            where T : IResponse
            where U : IRequest
        {
            var response =  await _privateClient.PostAsJsonAsync<U>(string.Format("{0}/Api/{1}", BaseUrl.TrimEnd('/'), call), requestData);
            return await response.Content.ReadAsAsync<T>();
        }

        public T GetResult<T, U>(PrivateApiCall call, U requestData)
            where T : IResponse
            where U : IRequest
        {
            string requestContentBase64String = string.Empty;
            string jsonData = JsonConvert.SerializeObject(requestData);
            string url = string.Format("{0}/Api/{1}", BaseUrl.TrimEnd('/'), call);
            HttpWebRequest http = (HttpWebRequest)WebRequest.Create(new Uri(url));
            string requestUri = System.Web.HttpUtility.UrlEncode(http.RequestUri.AbsoluteUri.ToLower());
            http.Accept = "application/json";
            http.ContentType = "application/json";
            http.Method = "POST";
            Byte[] bytes = Encoding.UTF8.GetBytes(jsonData);

            if (bytes != null)
            {
                using (var md5 = MD5.Create())
                {
                    var requestContentHash = md5.ComputeHash(bytes);
                    requestContentBase64String = Convert.ToBase64String(requestContentHash);
                }
            }

            //create random nonce for each request
            var nonce = Guid.NewGuid().ToString("N");

            //Creating the raw signature string
            var signatureRawData = string.Concat(ApiKey, "POST", requestUri, nonce, requestContentBase64String);
            var secretKeyByteArray = Convert.FromBase64String(ApiSecret);
            var signature = Encoding.UTF8.GetBytes(signatureRawData);
            using (var hmac = new HMACSHA256(secretKeyByteArray))
            {
                var signatureBytes = hmac.ComputeHash(signature);
                var requestSignatureBase64String = Convert.ToBase64String(signatureBytes);

                //Setting the values in the Authorization header using custom scheme (amx)
                //http.Headers.Authorization = new AuthenticationHeaderValue("amx", string.Format("{0}:{1}:{2}", ApiKey, requestSignatureBase64String, nonce));

                http.Headers.Add("Authorization: amx " + string.Format("{0}:{1}:{2}", ApiKey, requestSignatureBase64String, nonce));
            }


            Stream newStream = http.GetRequestStream();
            newStream.Write(bytes, 0, bytes.Length);
            newStream.Close();

            var response = http.GetResponse();

            var stream = response.GetResponseStream();
            var sr = new StreamReader(stream);
            var content = sr.ReadToEnd();

            return JsonConvert.DeserializeObject<T>(content);


        }

        public CancelTradeResponse CancelTrade(CancelTradeRequest request)
        {
            return GetResult<CancelTradeResponse, CancelTradeRequest>(PrivateApiCall.CancelTrade, request);
        }

        public SubmitTradeResponse SubmitTrade(SubmitTradeRequest request)
        {
            return GetResult<SubmitTradeResponse, SubmitTradeRequest>(PrivateApiCall.SubmitTrade, request);
        }

        public BalanceResponse GetBalances(BalanceRequest request)
        {
            return GetResult<BalanceResponse, BalanceRequest>(PrivateApiCall.GetBalance, request);
        }

        public OpenOrdersResponse GetOpenOrders(OpenOrdersRequest request)
        {
            return GetResult<OpenOrdersResponse, OpenOrdersRequest>(PrivateApiCall.GetOpenOrders, request);
        }

        public TradeHistoryResponse GetTradeHistory(TradeHistoryRequest request)
        {
            return GetResult<TradeHistoryResponse, TradeHistoryRequest>(PrivateApiCall.GetTradeHistory, request);
        }

        public TransactionResponse GetTransactions(TransactionRequest request)
        {
            return GetResult<TransactionResponse, TransactionRequest>(PrivateApiCall.GetTransactions, request);
        }

        public DepositAddressResponse GetDepositAddress(DepositAddressRequest request)
        {
            return GetResult<DepositAddressResponse, DepositAddressRequest>(PrivateApiCall.GetDepositAddress, request);
        }

        public SubmitWithdrawResponse SubmitWithdraw(SubmitWithdrawRequest request)
        {
            return GetResult<SubmitWithdrawResponse, SubmitWithdrawRequest>(PrivateApiCall.SubmitWithdraw, request);
        }

        #endregion

        #region Shim Implementation

        public string DoPublic(PublicApiCall call, string result) { return null; }

        public string DoPrivate(PrivateApiCall call, string result) { return null; }

        public string GetTradePairLabel(TradePair tradePair)
        {
            return string.Format("{0}_{1}", tradePair.CurrencyLabel, tradePair.BaseLabel);
        }


        #endregion

    }

    public class AuthDelegatingHandler : DelegatingHandler
    {
        private string _apiKey;
        private string _apiSecret;

        public AuthDelegatingHandler(string key, string secret)
        {
            _apiKey = key;
            _apiSecret = secret;
        }


        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string requestContentBase64String = string.Empty;
            string requestUri = System.Web.HttpUtility.UrlEncode(request.RequestUri.AbsoluteUri.ToLower());

            //Checking if the request contains body, usually will be null wiht HTTP GET
            if (request.Content != null)
            {
                using (var md5 = MD5.Create())
                {
                    var content = await request.Content.ReadAsByteArrayAsync();

                    //Hashing the request body, any change in request body will result in different hash, we'll incure message integrity
                    var requestContentHash = md5.ComputeHash(content);
                    requestContentBase64String = Convert.ToBase64String(requestContentHash);
                }
            }

            //create random nonce for each request
            var nonce = Guid.NewGuid().ToString("N");

            //Creating the raw signature string
            var signatureRawData = string.Concat(_apiKey, "POST", requestUri, nonce, requestContentBase64String);
            var secretKeyByteArray = Convert.FromBase64String(_apiSecret);
            var signature = Encoding.UTF8.GetBytes(signatureRawData);
            using (var hmac = new HMACSHA256(secretKeyByteArray))
            {
                var signatureBytes = hmac.ComputeHash(signature);
                var requestSignatureBase64String = Convert.ToBase64String(signatureBytes);

                //Setting the values in the Authorization header using custom scheme (amx)
                request.Headers.Authorization = new AuthenticationHeaderValue("amx", string.Format("{0}:{1}:{2}", _apiKey, requestSignatureBase64String, nonce));
            }
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
