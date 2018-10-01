using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchangeLib.Common
{

    #region Communication Interfaces

    public interface IRequest
    {

    }

    public interface IResponse
    {
        bool Success { get; set; }
        string Error { get; set; }
    }

    public class Response<T> : IResponse
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public T Data { get; set; }
        
    }


    #endregion

    #region Communication Implementations

    #region Public

    #region Requests

    public class MarketHistoryRequest : IRequest
    {
        public MarketHistoryRequest(TradePair tradePair)
        {
            TradePair = tradePair;
        }
        public TradePair TradePair { get; set; }
    }

    public class MarketOrdersRequest : IRequest
    {
        public MarketOrdersRequest(TradePair tradePair, int? orderCount = null)
        {
            TradePair = tradePair;
            OrderCount = orderCount;
        }
        public TradePair TradePair { get; set; }
        public int? OrderCount { get; set; }
    }

    public class MarketRequest : IRequest
    {
        public MarketRequest(TradePair tradePair, int? hours = null)
        {
            TradePair = tradePair;
            Hours = hours;
        }
        public TradePair TradePair { get; set; }
        public int? Hours { get; set; }
    }

    public class MarketsRequest : IRequest
    {
        public MarketsRequest(int? hours = null)
        {
            Hours = hours;
        }
        public int? Hours { get; set; }
    }



    #endregion

    #region Responses

    public class CurrenciesResponse : Response<List<CurrencyResult>> { }

    public class MarketHistoryResponse : Response<List<MarketHistoryResult>> { }

    public class MarketOrdersResponse : Response<MarketOrdersResult> { }

    public class MarketResponse : Response<MarketResult> { }

    public class MarketsResponse : Response<List<MarketResult>> { }

    public class TradePairsResponse : Response<List<TradePairResult>> { }

    #endregion


    #endregion

    #region Private

    #region Requests

    public class BalanceRequest : IRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BalanceRequest"/> class for an all balances request.
        /// </summary>
        public BalanceRequest()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BalanceRequest"/> class for a specific currency request.
        /// </summary>
        /// <param name="currency">The currency symbol of the balance to return.</param>
        public BalanceRequest(string currency)
        {
            Currency = currency;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BalanceRequest"/> class for a specific currency request.
        /// </summary>
        /// <param name="currencyId">The Cryptopia currency identifier balance to return.</param>
        public BalanceRequest(int currencyId)
        {
            CurrencyId = currencyId;
        }

        /// <summary>
        /// Gets or sets the currency symbol.
        /// </summary>
        public string Currency { get; set; }


        /// <summary>
        /// Gets or sets the Cryptopia currency identifier.
        /// </summary>
        public int? CurrencyId { get; set; }
    }

    public class CancelTradeRequest : IRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CancelTradeRequest"/> class for an cancel all request.
        /// </summary>
        public CancelTradeRequest()
        {
            Type = CancelTradeType.All;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CancelTradeRequest"/> class for a specific trade cancel request.
        /// </summary>
        /// <param name="currency">The currency symbol of the trade order to cancel.</param>
        public CancelTradeRequest(int orderId)
        {
            OrderId = orderId;
            Type = CancelTradeType.Trade;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CancelTradeRequest"/> class for a specific tradepair cancel request.
        /// </summary>
        /// <param name="currencyId">The Cryptopia currency identifier tradepar to cancel.</param>
        public CancelTradeRequest(int tradePairId, CancelTradeType type = CancelTradeType.TradePair)
        {
            TradePairId = tradePairId;
            Type = CancelTradeType.TradePair;
        }

        /// <summary>
        /// Gets or sets the trade identifier.
        /// </summary>
        public int? OrderId { get; set; }

        /// <summary>
        /// Gets or sets the Cryptopia tradepair identifier.
        /// </summary>
        public int? TradePairId { get; set; }

        /// <summary>
        /// Gets or sets the type of cancel.
        /// </summary>
        public CancelTradeType Type { get; set; }
    }

    public class DepositAddressRequest : IRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DepositAddressRequest"/> class for a specific currency request.
        /// </summary>
        /// <param name="currency">The currency symbol of the address to return.</param>
        public DepositAddressRequest(string currency)
        {
            Currency = currency;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DepositAddressRequest"/> class for a specific currency request.
        /// </summary>
        /// <param name="currencyId">The Cryptopia currency identifier address to return.</param>
        public DepositAddressRequest(int currencyId)
        {
            CurrencyId = currencyId;
        }

        /// <summary>
        /// Gets or sets the currency symbol.
        /// </summary>
        public string Currency { get; set; }


        /// <summary>
        /// Gets or sets the Cryptopia currency identifier.
        /// </summary>
        public int? CurrencyId { get; set; }
    }

    public class OpenOrdersRequest : IRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenOrdersRequest" /> class for an all open orders request.
        /// </summary>
        /// <param name="count">The count of records to return.</param>
        public OpenOrdersRequest(int? count = null)
        {
            Count = count;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenOrdersRequest"/> class for a specific open orders request.
        /// </summary>
        /// <param name="market">The market symbol of the orders to return.</param>
        /// <param name="count">The count of records to return.</param>
        public OpenOrdersRequest(string market, int? count = null)
        {
            Count = count;
            Market = market;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenOrdersRequest"/> class for a specific open orders request.
        /// </summary>
        /// <param name="tradePairId">The Cryptopia tradepair identifier orders to return.</param>
        /// <param name="count">The count of records to return.</param>
        public OpenOrdersRequest(int tradePairId, int? count = null)
        {
            Count = count;
            TradePairId = tradePairId;
        }

        /// <summary>
        /// Gets or sets the cryptopia trade pair identifier.
        /// </summary>
        public int? TradePairId { get; set; }

        /// <summary>
        /// Gets or sets the market identifier.
        /// </summary>
        public string Market { get; set; }

        /// <summary>
        /// Gets or sets the record count.
        /// </summary>
        public int? Count { get; set; }
    }

    public class SubmitTradeRequest : IRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitTradeRequest"/> class.
        /// </summary>
        /// <param name="market">The Cryptopia market identifier.</param>
        /// <param name="type">The type of trade.</param>
        /// <param name="amount">The amount of coins.</param>
        /// <param name="rate">The price of the coins.</param>
        public SubmitTradeRequest(string market, TradeType type, decimal amount, decimal rate)
        {
            Market = market;
            Type = type;
            Amount = amount;
            Rate = rate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitTradeRequest"/> class.
        /// </summary>
        /// <param name="tradepairId">The Cryptopia tradepair identifier.</param>
        /// <param name="type">The type of trade.</param>
        /// <param name="amount">The amount of coins.</param>
        /// <param name="rate">The price of the coins.</param>
        public SubmitTradeRequest(int tradepairId, TradeType type, decimal amount, decimal rate)
        {
            TradePairId = tradepairId;
            Type = type;
            Amount = amount;
            Rate = rate;
        }

        /// <summary>
        /// Gets or sets the Cryptopia trade pair identifier.
        /// </summary>
        public int? TradePairId { get; set; }

        /// <summary>
        /// Gets or sets the market symbol.
        /// </summary>
        public string Market { get; set; }

        /// <summary>
        /// Gets or sets the trade type.
        /// </summary>
        public TradeType Type { get; set; }

        /// <summary>
        /// Gets or sets the rate/price.
        /// </summary>
        public decimal Rate { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        public decimal Amount { get; set; }
    }

    public class SubmitWithdrawRequest : IRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitWithdrawRequest"/> class.
        /// </summary>
        /// <param name="currency">The currency symbol.</param>
        /// <param name="amount">The total amount of coins to spend (amount will be divided equally amongst the active users).</param>
        /// <param name="address">The receiving address (address must belong in you Cryptopia Addressbook)..</param>
        public SubmitWithdrawRequest(string currency, decimal amount, string address)
        {
            Currency = currency;
            Address = address;
            Amount = amount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitWithdrawRequest"/> class.
        /// </summary>
        /// <param name="currencyId">The currency identifier.</param>
        /// <param name="amount">The total amount of coins to spend (amount will be divided equally amongst the active users).</param>
        /// <param name="address">The receiving address (address must belong in you Cryptopia Addressbook)..</param>
        public SubmitWithdrawRequest(int currencyId, decimal amount, string address)
        {
            CurrencyId = currencyId;
            Address = address;
            Amount = amount;
        }

        /// <summary>
        /// Gets or sets the total amount of coins to withdraw.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the currency.
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets the currency identifier.
        /// </summary>
        public int? CurrencyId { get; set; }

        /// <summary>
        /// Gets or sets the address (address must belong in you Cryptopia Addressbook).
        /// </summary>
        public string Address { get; set; }
    }

    public class TradeHistoryRequest : IRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TradeHistoryRequest" /> class for an all trade history request.
        /// </summary>
        /// <param name="count">The count of records to return.</param>
        public TradeHistoryRequest(int? count = null)
        {
            Count = count;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TradeHistoryRequest"/> class for a market specific trade history request.
        /// </summary>
        /// <param name="market">The market symbol of the trade history to return.</param>
        /// <param name="count">The count of records to return.</param>
        public TradeHistoryRequest(string market, int? count = null)
        {
            Count = count;
            Market = market;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TradeHistoryRequest"/> class for a tradepair specific trade history request.
        /// </summary>
        /// <param name="tradePairId">The Cryptopia tradepair identifier trade history to return.</param>
        /// <param name="count">The count of records to return.</param>
        public TradeHistoryRequest(int tradePairId, int? count = null)
        {
            Count = count;
            TradePairId = tradePairId;
        }

        /// <summary>
        /// Gets or sets the cryptopia trade pair identifier.
        /// </summary>
        public int? TradePairId { get; set; }

        /// <summary>
        /// Gets or sets the market identifier.
        /// </summary>
        public string Market { get; set; }

        /// <summary>
        /// Gets or sets the record count.
        /// </summary>
        public int? Count { get; set; }
    }

    public class TransactionRequest : IRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionRequest"/> class of returning Withdrawl/Deposit transactions.
        /// </summary>
        /// <param name="type">The type of transactions to return.</param>
        /// <param name="count">The count of records.</param>
        public TransactionRequest(TransactionType type, int? count = null)
        {
            Type = type;
            Count = count;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionRequest"/> class for returning Withdrawl/Deposit transactions
        /// </summary>
        /// <param name="currency">The currency for the transactions to return.</param>
        public TransactionRequest(string currency)
        {
            Currency = currency;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionRequest"/> class for returning Withdrawl/Deposit transactions
        /// </summary>
        public TransactionRequest() { }

        /// <summary>
        /// Gets or sets the type of transactions.
        /// </summary>
        public TransactionType Type { get; set; }

        /// <summary>
        /// Gets or sets the max count of records to return.
        /// </summary>
        public int? Count { get; set; }

        ///<summary>
        ///Gets or sets the Currency identifier
        /// </summary>
        public string Currency { get; set; }
    }

    #endregion

    #region Responses

    public class BalanceResponse : Response<List<BalanceResult>> { }

    public class CancelTradeResponse : Response<List<int>> { }

    public class DepositAddressResponse : Response<DepositAddress> { }

    public class OpenOrdersResponse : Response<List<OpenOrderResult>> { }

    public class SubmitTradeResponse : Response<SubmitTradeData> { }

    public class SubmitWithdrawResponse : Response<int?> { }

    public class TradeHistoryResponse : Response<List<TradeHistoryResult>> { }

    public class TransactionResponse : Response<List<TransactionResult>> { }

    #endregion


    #endregion

    #region Misc Data Classes

    public enum CancelTradeType
    {
        /// <summary>
        /// Single open order cancel
        /// </summary>
        Trade,

        /// <summary>
        /// Cancel all open orders for tradepair
        /// </summary>
        TradePair,

        /// <summary>
        /// Cancel All open orders
        /// </summary>
        All
    }

    public class DepositAddress
    {
        public string Currency { get; set; }
        public string Address { get; set; }
    }

    public class SubmitTradeData
    {
        public int? OrderId { get; set; }
        public List<int> FilledOrders { get; set; }
    }

    public class TradePair
    {
        public int? Id { get; set; }
        public string BaseLabel { get; set; }
        public string CurrencyLabel { get; set; }
        public string PairLabel { get { return BaseLabel + "-" + CurrencyLabel; } }
        
    }

    public enum TradeType
    {
        Buy,
        Sell
    }

    public enum TransactionType
    {
        Deposit,
        Withdraw
    }

    #endregion

    #region Results

    public class BalanceResult
    {
        public int CurrencyId { get; set; }
        public string Symbol { get; set; }
        public decimal Total { get; set; }
        public decimal Available { get; set; }
        public decimal Unconfirmed { get; set; }
        public decimal HeldForTrades { get; set; }
        public decimal PendingWithdraw { get; set; }
        public string Address { get; set; }
        public string BaseAddress { get; set; }
        public string Status { get; set; }
        public string StatusMessage { get; set; }
    }

    public class CurrencyResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public string Algorithm { get; set; }
        public decimal WithdrawFee { get; set; }
        public decimal MinWithdraw { get; set; }
        public decimal MaxWithdraw { get; set; }
        public decimal MinBaseTrade { get; set; }
        public int DepositConfirmations { get; set; }
        public string Status { get; set; }
        public string StatusMessage { get; set; }
        public string ListingStatus { get; set; }
    }

    public class MarketHistoryResult
    {
        public int Id { get; set; }
        public string TradePairId { get; set; }
        public string Label { get; set; }
        public string Type { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public decimal Total { get; set; }
        public int Timestamp { get; set; }
    }

    public class MarketOrderResult
    {
        public string TradePairId { get; set; }
        public string Label { get; set; }
        public decimal Price { get; set; }
        public decimal Volume { get; set; }
        public decimal Total { get; set; }
    }

    public class MarketOrdersResult
    {
        public List<MarketOrderResult> Buy { get; set; }
        public List<MarketOrderResult> Sell { get; set; }
    }

    public class MarketResult
    {
        public string TradePairId { get; set; }
        public string Label { get; set; }
        public decimal AskPrice { get; set; }
        public decimal BidPrice { get; set; }
        public decimal Low { get; set; }
        public decimal High { get; set; }
        public decimal Volume { get; set; }
        public decimal LastPrice { get; set; }
        public decimal LastVolume { get; set; }
        public decimal BuyVolume { get; set; }
        public decimal SellVolume { get; set; }
        public decimal Change { get; set; }
        public decimal PrevDay { get; set; }
    }

    public class OpenOrderResult
    {
        public int OrderId { get; set; }
        public string TradePairId { get; set; }
        public string Market { get; set; }
        public string Type { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public decimal Total { get; set; }
        public decimal Remaining { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    public class TradeHistoryResult
    {
        public int TradeId { get; set; }
        public string TradePairId { get; set; }
        public string Market { get; set; }
        public string Type { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public decimal Total { get; set; }
        public decimal Fee { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    public class TradePairResult
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public string Currency { get; set; }
        public string Symbol { get; set; }
        public string BaseCurrency { get; set; }
        public string BaseSymbol { get; set; }
        public string Status { get; set; }
        public string StatusMessage { get; set; }
        public decimal TradeFee { get; set; }
        public decimal MinimumTrade { get; set; }
        public decimal MaximumTrade { get; set; }
        public decimal MinimumBaseTrade { get; set; }
        public decimal MaximumBaseTrade { get; set; }
        public decimal MinimumPrice { get; set; }
        public decimal MaximumPrice { get; set; }
    }

    public class TransactionResult
    {
        public string Address { get; set; }
        public int Id { get; set; }
        public string Currency { get; set; }
        public string TxId { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public decimal Fee { get; set; }
        public string Status { get; set; }
        public int Confirmations { get; set; }
        public DateTime Timestamp { get; set; }
    }

    #endregion

    #endregion

    #region API Call Enums

    public enum PrivateApiCall
    {
        CancelTrade,
        GetTradeHistory,
        GetOpenOrders,
        GetBalance,
        SubmitTrade,
        GetTransactions,
        SubmitTip,
        GetDepositAddress,
        SubmitWithdraw
    }

    public enum PublicApiCall
    {
        GetCurrencies,
        GetTradePairs,
        GetMarkets,
        GetMarket,
        GetMarketHistory,
        GetMarketOrders
    }

    #endregion

    #region IExchange Interface

    public interface IExchange : IDisposable
    {
        string ApiKey { get; set; }
        string ApiSecret { get; set; }
        string BaseUrl { get; set; }

        #region Public API Methods

        T GetResult<T>(PublicApiCall call, string requestData) where T : IResponse, new();
        CurrenciesResponse GetCurrencies();
        MarketResponse GetMarket(MarketRequest request);
        MarketHistoryResponse GetMarketHistory(MarketHistoryRequest request);
        MarketOrdersResponse GetMarketOrders(MarketOrdersRequest request);
        MarketsResponse GetMarkets(MarketsRequest request);
        TradePairsResponse GetTradePairs();

        #endregion

        #region Private API Methods

        T GetResult<T, U>(PrivateApiCall call, U requestData)
            where T : IResponse
            where U : IRequest;
        CancelTradeResponse CancelTrade(CancelTradeRequest request);
        BalanceResponse GetBalances(BalanceRequest request);
        DepositAddressResponse GetDepositAddress(DepositAddressRequest request);
        OpenOrdersResponse GetOpenOrders(OpenOrdersRequest request);
        TradeHistoryResponse GetTradeHistory(TradeHistoryRequest request);
        TransactionResponse GetTransactions(TransactionRequest request);
        SubmitTradeResponse SubmitTrade(SubmitTradeRequest request);
        SubmitWithdrawResponse SubmitWithdraw(SubmitWithdrawRequest request);

        #endregion

        #region Shim Methods

        string DoPublic(PublicApiCall call, string response);
        string DoPrivate(PrivateApiCall call, string response);
        string GetTradePairLabel(TradePair tradePair);

        #endregion

    }

    #endregion


    #region Converter Class

    public static class Converter
    {

        public static Dictionary<string, CurrencyResult> ToDictionary(CurrenciesResponse response)
        {
            Dictionary<string, CurrencyResult> d = new Dictionary<string, CurrencyResult>();

            foreach(var c in response.Data)
            {
                d.Add(c.Symbol, c);
            }

            return d;
        }

        public static Dictionary<string, TradePairResult> ToDictionary(TradePairsResponse response)
        {
            Dictionary<string, TradePairResult> d = new Dictionary<string, TradePairResult>();

            foreach(var c in response.Data)
            {
                d.Add(c.Label, c);
            }

            return d;
        }


        public static Dictionary<string, MarketResult> ToDictionary(MarketsResponse response)
        {
            Dictionary<string, MarketResult> d = new Dictionary<string, MarketResult>();

            foreach(var c in response.Data)
            {
                d.Add(c.Label, c);
            }

            return d;
        }

    }

    #endregion
}
