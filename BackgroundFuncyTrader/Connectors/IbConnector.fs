namespace Connectors

open IBApi
open System.Threading
open Microsoft.Extensions.Logging

type IbSettings = {
    Host: string
    Port: int
    ClientId: int }

type IConnector =
    abstract Connect: unit -> unit

type IbConnector =
    val log: ILogger<IbConnector>
    val setting: IbSettings
    val signal: EReaderMonitorSignal
    val client: EClientSocket

    new(settings: IbSettings, logger: ILogger<IbConnector>) as self = {
        setting = settings
        log = logger
        signal = EReaderMonitorSignal()
        client = EClientSocket(self, self.signal)
    }
        
    interface IConnector with
        member this.Connect() = 
            this.client.eConnect("127.0.0.1", 7497, 1000)
            let reader = EReader(this.client, this.signal).Start()
            let th = Thread(ThreadStart(fun _ -> while this.client.IsConnected() do
                                                        this.signal.waitForSignal()
                                                        reader.processMsgs()))
            th.IsBackground <- true
            th.Start()

    interface EWrapper with
        member this.accountDownloadEnd(account) = raise (System.NotImplementedException())
        member this.accountSummary(reqId, account, tag, value, currency) = raise (System.NotImplementedException())
        member this.accountSummaryEnd(reqId) = raise (System.NotImplementedException())
        member this.accountUpdateMulti(requestId, account, modelCode, key, value, currency) = raise (System.NotImplementedException())
        member this.accountUpdateMultiEnd(requestId) = raise (System.NotImplementedException())
        member this.bondContractDetails(reqId, contract) = raise (System.NotImplementedException())
        member this.commissionReport(commissionReport) = raise (System.NotImplementedException())
        member this.completedOrder(contract, order, orderState) = raise (System.NotImplementedException())
        member this.completedOrdersEnd() = raise (System.NotImplementedException())
        member this.connectAck() = ()
        member this.connectionClosed() = raise (System.NotImplementedException())
        member this.contractDetails(reqId, contractDetails) = raise (System.NotImplementedException())
        member this.contractDetailsEnd(reqId) = raise (System.NotImplementedException())
        member this.currentTime(time) = raise (System.NotImplementedException())
        member this.deltaNeutralValidation(reqId, deltaNeutralContract) = raise (System.NotImplementedException())
        member this.displayGroupList(reqId, groups) = raise (System.NotImplementedException())
        member this.displayGroupUpdated(reqId, contractInfo) = raise (System.NotImplementedException())
        member this.error(e: exn): unit = this.log.LogError(e.Message)
        member this.error(str: string): unit = this.log.LogError(str)
        member this.error(id: int, errorCode: int, errorMsg: string, advancedOrderRejectJson: string): unit = 
            match errorCode with 
            | 2158 -> this.client.reqMarketDataType(3)
            | _ -> this.log.LogError("{id}: {code}: {msg}", id, errorCode, errorMsg)

        member this.execDetails(reqId, contract, execution) = raise (System.NotImplementedException())
        member this.execDetailsEnd(reqId) = raise (System.NotImplementedException())
        member this.familyCodes(familyCodes) = raise (System.NotImplementedException())
        member this.fundamentalData(reqId, data) = raise (System.NotImplementedException())
        member this.headTimestamp(reqId, headTimestamp) = raise (System.NotImplementedException())
        member this.histogramData(reqId, data) = raise (System.NotImplementedException())
        member this.historicalData(reqId, bar) = raise (System.NotImplementedException())
        member this.historicalDataEnd(reqId, start, ``end``) = raise (System.NotImplementedException())
        member this.historicalDataUpdate(reqId, bar) = raise (System.NotImplementedException())
        member this.historicalNews(requestId, time, providerCode, articleId, headline) = raise (System.NotImplementedException())
        member this.historicalNewsEnd(requestId, hasMore) = raise (System.NotImplementedException())
        member this.historicalSchedule(reqId, startDateTime, endDateTime, timeZone, sessions) = raise (System.NotImplementedException())
        member this.historicalTicks(reqId, ticks, ``done``) = raise (System.NotImplementedException())
        member this.historicalTicksBidAsk(reqId, ticks, ``done``) = raise (System.NotImplementedException())
        member this.historicalTicksLast(reqId, ticks, ``done``) = raise (System.NotImplementedException())
        member this.managedAccounts(accountsList) = raise (System.NotImplementedException())
        member this.marketDataType(reqId, marketDataType) = raise (System.NotImplementedException())
        member this.marketRule(marketRuleId, priceIncrements) = raise (System.NotImplementedException())
        member this.mktDepthExchanges(depthMktDataDescriptions) = raise (System.NotImplementedException())
        member this.newsArticle(requestId, articleType, articleText) = raise (System.NotImplementedException())
        member this.newsProviders(newsProviders) = raise (System.NotImplementedException())
        member this.nextValidId(orderId) = raise (System.NotImplementedException())
        member this.openOrder(orderId, contract, order, orderState) = raise (System.NotImplementedException())
        member this.openOrderEnd() = raise (System.NotImplementedException())
        member this.orderBound(orderId, apiClientId, apiOrderId) = raise (System.NotImplementedException())
        member this.orderStatus(orderId, status, filled, remaining, avgFillPrice, permId, parentId, lastFillPrice, clientId, whyHeld, mktCapPrice) = raise (System.NotImplementedException())
        member this.pnl(reqId, dailyPnL, unrealizedPnL, realizedPnL) = raise (System.NotImplementedException())
        member this.pnlSingle(reqId, pos, dailyPnL, unrealizedPnL, realizedPnL, value) = raise (System.NotImplementedException())
        member this.position(account, contract, pos, avgCost) = raise (System.NotImplementedException())
        member this.positionEnd() = raise (System.NotImplementedException())
        member this.positionMulti(requestId, account, modelCode, contract, pos, avgCost) = raise (System.NotImplementedException())
        member this.positionMultiEnd(requestId) = raise (System.NotImplementedException())
        member this.realtimeBar(reqId, date, ``open``, high, low, close, volume, wAP, count) = raise (System.NotImplementedException())
        member this.receiveFA(faDataType, faXmlData) = raise (System.NotImplementedException())
        member this.replaceFAEnd(reqId, text) = raise (System.NotImplementedException())
        member this.rerouteMktDataReq(reqId, conId, exchange) = raise (System.NotImplementedException())
        member this.rerouteMktDepthReq(reqId, conId, exchange) = raise (System.NotImplementedException())
        member this.scannerData(reqId, rank, contractDetails, distance, benchmark, projection, legsStr) = raise (System.NotImplementedException())
        member this.scannerDataEnd(reqId) = raise (System.NotImplementedException())
        member this.scannerParameters(xml) = raise (System.NotImplementedException())
        member this.securityDefinitionOptionParameter(reqId, exchange, underlyingConId, tradingClass, multiplier, expirations, strikes) = raise (System.NotImplementedException())
        member this.securityDefinitionOptionParameterEnd(reqId) = raise (System.NotImplementedException())
        member this.smartComponents(reqId, theMap) = raise (System.NotImplementedException())
        member this.softDollarTiers(reqId, tiers) = raise (System.NotImplementedException())
        member this.symbolSamples(reqId, contractDescriptions) = raise (System.NotImplementedException())
        member this.tickByTickAllLast(reqId, tickType, time, price, size, tickAttribLast, exchange, specialConditions) = raise (System.NotImplementedException())
        member this.tickByTickBidAsk(reqId, time, bidPrice, askPrice, bidSize, askSize, tickAttribBidAsk) = raise (System.NotImplementedException())
        member this.tickByTickMidPoint(reqId, time, midPoint) = raise (System.NotImplementedException())
        member this.tickEFP(tickerId, tickType, basisPoints, formattedBasisPoints, impliedFuture, holdDays, futureLastTradeDate, dividendImpact, dividendsToLastTradeDate) = raise (System.NotImplementedException())
        member this.tickGeneric(tickerId, field, value) = raise (System.NotImplementedException())
        member this.tickNews(tickerId, timeStamp, providerCode, articleId, headline, extraData) = raise (System.NotImplementedException())
        member this.tickOptionComputation(tickerId, field, tickAttrib, impliedVolatility, delta, optPrice, pvDividend, gamma, vega, theta, undPrice) = raise (System.NotImplementedException())
        member this.tickPrice(tickerId, field, price, attribs) = raise (System.NotImplementedException())
        member this.tickReqParams(tickerId, minTick, bboExchange, snapshotPermissions) = raise (System.NotImplementedException())
        member this.tickSize(tickerId, field, size) = raise (System.NotImplementedException())
        member this.tickSnapshotEnd(tickerId) = raise (System.NotImplementedException())
        member this.tickString(tickerId, field, value) = raise (System.NotImplementedException())
        member this.updateAccountTime(timestamp) = raise (System.NotImplementedException())
        member this.updateAccountValue(key, value, currency, accountName) = raise (System.NotImplementedException())
        member this.updateMktDepth(tickerId, position, operation, side, price, size) = raise (System.NotImplementedException())
        member this.updateMktDepthL2(tickerId, position, marketMaker, operation, side, price, size, isSmartDepth) = raise (System.NotImplementedException())
        member this.updateNewsBulletin(msgId, msgType, message, origExchange) = raise (System.NotImplementedException())
        member this.updatePortfolio(contract, position, marketPrice, marketValue, averageCost, unrealizedPNL, realizedPNL, accountName) = raise (System.NotImplementedException())
        member this.userInfo(reqId, whiteBrandingId) = raise (System.NotImplementedException())
        member this.verifyAndAuthCompleted(isSuccessful, errorText) = raise (System.NotImplementedException())
        member this.verifyAndAuthMessageAPI(apiData, xyzChallenge) = raise (System.NotImplementedException())
        member this.verifyCompleted(isSuccessful, errorText) = raise (System.NotImplementedException())
        member this.verifyMessageAPI(apiData) = raise (System.NotImplementedException())
        member this.wshEventData(reqId, dataJson) = raise (System.NotImplementedException())
        member this.wshMetaData(reqId, dataJson) = raise (System.NotImplementedException())