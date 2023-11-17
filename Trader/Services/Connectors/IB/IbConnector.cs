using IBApi;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using Trader.Models;

namespace Trader.Services.Connectors.IB;

public class IbConnector : IConnector, EWrapper {
    private readonly IbSettings settings;
    private readonly ILogger<IConnector> logger;
    private readonly EReaderMonitorSignal signal;
    private readonly EClientSocket client;
    private int validOrderId;

    public event FutureEventHandler FutureEvent = delegate { };
    public event PriceEventHandler LastPriceEvent = delegate { };
    public IbConnector(ILogger<IConnector> logger, ISettings settings) {
        signal = new EReaderMonitorSignal();
        client = new EClientSocket(this, signal);

        this.settings = settings.GetIbSettings()
                        ?? throw new ArgumentNullException(nameof(settings));
        this.logger = logger;
    }

    public bool IsConnected => client.IsConnected();

    public void RequestFuture(int reqId, string symbol, string exchange, DateOnly expiration) {
        var contract = new Contract() {
            Symbol = symbol,
            Exchange = exchange,
            LastTradeDateOrContractMonth = expiration.ToIbDateTime(),
            SecType = "FUT"
        };
        client.reqContractDetails(reqId, contract);
    }
    public void RequestMarketData(Instrument instrument) {
        client.reqMktData(instrument.Id, instrument.ToContract(), string.Empty, false, false, null);
    }

    public void Connect() {
        client.eConnect(settings.Host, settings.Port, settings.ClientId);
        var reader = new EReader(client, signal).Start();

        new Thread(() => {
            while (client.IsConnected()) {
                signal.waitForSignal();
                reader.processMsgs();
            }
        }) { IsBackground = true }.Start();
    }

    #region EWrapper
    public void accountDownloadEnd(string account) {
        throw new NotImplementedException();
    }

    public void accountSummary(int reqId, string account, string tag, string value, string currency) {
        throw new NotImplementedException();
    }

    public void accountSummaryEnd(int reqId) {
        throw new NotImplementedException();
    }

    public void accountUpdateMulti(int requestId, string account, string modelCode, string key, string value, string currency) {
        throw new NotImplementedException();
    }

    public void accountUpdateMultiEnd(int requestId) {
        throw new NotImplementedException();
    }

    public void bondContractDetails(int reqId, ContractDetails contract) {
        throw new NotImplementedException();
    }

    public void commissionReport(CommissionReport commissionReport) {
        throw new NotImplementedException();
    }

    public void completedOrder(Contract contract, Order order, OrderState orderState) {
        throw new NotImplementedException();
    }

    public void completedOrdersEnd() {
        throw new NotImplementedException();
    }

    public void connectAck() { }
    public void connectionClosed() {
        throw new NotImplementedException();
    }

    public void contractDetails(int reqId, ContractDetails contractDetails) {
        if (contractDetails.Contract.SecType == "FUT") {
            var instrument = contractDetails.ToInstrument();
            FutureEvent.Invoke(reqId, this, instrument);
        }
    }

    public void contractDetailsEnd(int reqId) { }

    public void currentTime(long time) {
        throw new NotImplementedException();
    }

    public void deltaNeutralValidation(int reqId, DeltaNeutralContract deltaNeutralContract) {
        throw new NotImplementedException();
    }

    public void displayGroupList(int reqId, string groups) {
        throw new NotImplementedException();
    }

    public void displayGroupUpdated(int reqId, string contractInfo) {
        throw new NotImplementedException();
    }

    public void error(Exception e) => logger.LogCritical(e.Message);

    public void error(string str) => logger.LogError(str);

    public void error(int id, int errorCode, string errorMsg, string advancedOrderRejectJson) {
        switch (errorCode) {
            case 2158:
                client.reqMarketDataType(3);
                break;
            default:
                break;
        }
        logger.LogError("{id}: {errorCode}: {errorMsg}", id, errorCode, errorMsg);
    }

    public void execDetails(int reqId, Contract contract, Execution execution) {
        throw new NotImplementedException();
    }

    public void execDetailsEnd(int reqId) {
        throw new NotImplementedException();
    }

    public void familyCodes(FamilyCode[] familyCodes) {
        throw new NotImplementedException();
    }

    public void fundamentalData(int reqId, string data) {
        throw new NotImplementedException();
    }

    public void headTimestamp(int reqId, string headTimestamp) {
        throw new NotImplementedException();
    }

    public void histogramData(int reqId, HistogramEntry[] data) {
        throw new NotImplementedException();
    }

    public void historicalData(int reqId, Bar bar) {
        throw new NotImplementedException();
    }

    public void historicalDataEnd(int reqId, string start, string end) {
        throw new NotImplementedException();
    }

    public void historicalDataUpdate(int reqId, Bar bar) {
        throw new NotImplementedException();
    }

    public void historicalNews(int requestId, string time, string providerCode, string articleId, string headline) {
        throw new NotImplementedException();
    }

    public void historicalNewsEnd(int requestId, bool hasMore) {
        throw new NotImplementedException();
    }

    public void historicalSchedule(int reqId, string startDateTime, string endDateTime, string timeZone, HistoricalSession[] sessions) {
        throw new NotImplementedException();
    }

    public void historicalTicks(int reqId, HistoricalTick[] ticks, bool done) {
        throw new NotImplementedException();
    }

    public void historicalTicksBidAsk(int reqId, HistoricalTickBidAsk[] ticks, bool done) {
        throw new NotImplementedException();
    }

    public void historicalTicksLast(int reqId, HistoricalTickLast[] ticks, bool done) {
        throw new NotImplementedException();
    }

    public void managedAccounts(string accountsList) { }
    public void marketDataType(int reqId, int marketDataType) { }

    public void marketRule(int marketRuleId, PriceIncrement[] priceIncrements) {
        throw new NotImplementedException();
    }

    public void mktDepthExchanges(DepthMktDataDescription[] depthMktDataDescriptions) {
        throw new NotImplementedException();
    }

    public void newsArticle(int requestId, int articleType, string articleText) {
        throw new NotImplementedException();
    }

    public void newsProviders(NewsProvider[] newsProviders) {
        throw new NotImplementedException();
    }
    public void nextValidId(int orderId) => validOrderId = orderId;
    public void openOrder(int orderId, Contract contract, Order order, OrderState orderState) {
        throw new NotImplementedException();
    }

    public void openOrderEnd() {
        throw new NotImplementedException();
    }

    public void orderBound(long orderId, int apiClientId, int apiOrderId) {
        throw new NotImplementedException();
    }

    public void orderStatus(int orderId, string status, decimal filled, decimal remaining, double avgFillPrice, int permId, int parentId, double lastFillPrice, int clientId, string whyHeld, double mktCapPrice) {
        throw new NotImplementedException();
    }

    public void pnl(int reqId, double dailyPnL, double unrealizedPnL, double realizedPnL) {
        throw new NotImplementedException();
    }

    public void pnlSingle(int reqId, decimal pos, double dailyPnL, double unrealizedPnL, double realizedPnL, double value) {
        throw new NotImplementedException();
    }

    public void position(string account, Contract contract, decimal pos, double avgCost) {
        throw new NotImplementedException();
    }

    public void positionEnd() {
        throw new NotImplementedException();
    }

    public void positionMulti(int requestId, string account, string modelCode, Contract contract, decimal pos, double avgCost) {
        throw new NotImplementedException();
    }

    public void positionMultiEnd(int requestId) {
        throw new NotImplementedException();
    }

    public void realtimeBar(int reqId, long date, double open, double high, double low, double close, decimal volume, decimal WAP, int count) {
        throw new NotImplementedException();
    }

    public void receiveFA(int faDataType, string faXmlData) {
        throw new NotImplementedException();
    }

    public void replaceFAEnd(int reqId, string text) {
        throw new NotImplementedException();
    }

    public void rerouteMktDataReq(int reqId, int conId, string exchange) {
        throw new NotImplementedException();
    }

    public void rerouteMktDepthReq(int reqId, int conId, string exchange) {
        throw new NotImplementedException();
    }

    public void scannerData(int reqId, int rank, ContractDetails contractDetails, string distance, string benchmark, string projection, string legsStr) {
        throw new NotImplementedException();
    }

    public void scannerDataEnd(int reqId) {
        throw new NotImplementedException();
    }

    public void scannerParameters(string xml) {
        throw new NotImplementedException();
    }

    public void securityDefinitionOptionParameter(int reqId, string exchange, int underlyingConId, string tradingClass, string multiplier, HashSet<string> expirations, HashSet<double> strikes) {
        throw new NotImplementedException();
    }

    public void securityDefinitionOptionParameterEnd(int reqId) {
        throw new NotImplementedException();
    }

    public void smartComponents(int reqId, Dictionary<int, KeyValuePair<string, char>> theMap) {
        throw new NotImplementedException();
    }

    public void softDollarTiers(int reqId, SoftDollarTier[] tiers) {
        throw new NotImplementedException();
    }

    public void symbolSamples(int reqId, ContractDescription[] contractDescriptions) {
        throw new NotImplementedException();
    }

    public void tickByTickAllLast(int reqId, int tickType, long time, double price, decimal size, TickAttribLast tickAttribLast, string exchange, string specialConditions) {
        throw new NotImplementedException();
    }

    public void tickByTickBidAsk(int reqId, long time, double bidPrice, double askPrice, decimal bidSize, decimal askSize, TickAttribBidAsk tickAttribBidAsk) {
        throw new NotImplementedException();
    }

    public void tickByTickMidPoint(int reqId, long time, double midPoint) {
        throw new NotImplementedException();
    }

    public void tickEFP(int tickerId, int tickType, double basisPoints, string formattedBasisPoints, double impliedFuture, int holdDays, string futureLastTradeDate, double dividendImpact, double dividendsToLastTradeDate) {
        throw new NotImplementedException();
    }

    public void tickGeneric(int tickerId, int field, double value) {
        throw new NotImplementedException();
    }

    public void tickNews(int tickerId, long timeStamp, string providerCode, string articleId, string headline, string extraData) {
        throw new NotImplementedException();
    }

    public void tickOptionComputation(int tickerId, int field, int tickAttrib, double impliedVolatility, double delta, double optPrice, double pvDividend, double gamma, double vega, double theta, double undPrice) {
        throw new NotImplementedException();
    }

    public void tickPrice(int tickerId, int field, double price, TickAttrib attribs) {
        if (price <= 0.0) return;
        if (price == double.MaxValue) return;

        switch (field) {
            case TickType.BID:
            case TickType.DELAYED_BID:
                break;
            case TickType.ASK:
            case TickType.DELAYED_ASK:
                break;
            case TickType.LAST:
            case TickType.DELAYED_LAST:
                break;
            default:
                break;
        }
    }

    public void tickReqParams(int tickerId, double minTick, string bboExchange, int snapshotPermissions) {
        /* пригодится */
    }

    public void tickSize(int tickerId, int field, decimal size) { }
    public void tickSnapshotEnd(int tickerId) { }
    public void tickString(int tickerId, int field, string value) { }

    public void updateAccountTime(string timestamp) {
        throw new NotImplementedException();
    }

    public void updateAccountValue(string key, string value, string currency, string accountName) {
        throw new NotImplementedException();
    }

    public void updateMktDepth(int tickerId, int position, int operation, int side, double price, decimal size) {
        throw new NotImplementedException();
    }

    public void updateMktDepthL2(int tickerId, int position, string marketMaker, int operation, int side, double price, decimal size, bool isSmartDepth) {
        throw new NotImplementedException();
    }

    public void updateNewsBulletin(int msgId, int msgType, string message, string origExchange) {
        throw new NotImplementedException();
    }

    public void updatePortfolio(Contract contract, decimal position, double marketPrice, double marketValue, double averageCost, double unrealizedPNL, double realizedPNL, string accountName) {
        throw new NotImplementedException();
    }

    public void userInfo(int reqId, string whiteBrandingId) {
        throw new NotImplementedException();
    }

    public void verifyAndAuthCompleted(bool isSuccessful, string errorText) {
        throw new NotImplementedException();
    }

    public void verifyAndAuthMessageAPI(string apiData, string xyzChallenge) {
        throw new NotImplementedException();
    }

    public void verifyCompleted(bool isSuccessful, string errorText) {
        throw new NotImplementedException();
    }

    public void verifyMessageAPI(string apiData) {
        throw new NotImplementedException();
    }

    public void wshEventData(int reqId, string dataJson) {
        throw new NotImplementedException();
    }

    public void wshMetaData(int reqId, string dataJson) {
        throw new NotImplementedException();
    }
    #endregion
}
