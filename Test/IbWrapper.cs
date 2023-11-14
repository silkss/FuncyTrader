using IBApi;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Test;

internal class IbWrapper : DefaultEWrapper {
    event Action<int, double> askPriceEvent = delegate { };
    event Action<int, double> bidPriceEvent = delegate { };
    private void reqMarketDat(Contract contract) {
        client.reqMktData(contract.ConId, contract, string.Empty, true, false, null);
    }

    private class OptionWithPrice {
        private readonly Contract contract;

        public OptionWithPrice(Contract contract, IbWrapper wrapper) {
            this.contract = contract;
            wrapper.askPriceEvent += askPriceEventHandler;
            wrapper.bidPriceEvent += bidPriceEventHandler;

            wrapper.reqMarketDat(this.contract);
        }

        public double Bid;
        public double Ask;
        public double Model;

        private void askPriceEventHandler(int ticker, double price) {
            if (ticker != contract.ConId) return;
            Ask = price;
        }

        private void bidPriceEventHandler(int ticker, double price) {
            if (ticker != contract.ConId) return;
            Bid = price;
        }

        public void Print() {
            Console.WriteLine($"{contract.LocalSymbol,-20} {Bid} - {Ask}");
        }
    }

    private readonly EReaderMonitorSignal signal;
    private readonly EClientSocket client;
    private readonly List<OptionWithPrice> contracts;
    public IbWrapper() {
        signal = new EReaderMonitorSignal();
        client = new EClientSocket(this, signal);
        contracts = new();
    }

    public void Connect() {
        client.eConnect("127.0.0.1", 7497, 100);
        var reader = new EReader(client, signal);
        reader.Start();

        new Thread(() => {
            while (client.IsConnected()) {
                signal.waitForSignal();
                reader.processMsgs();
            }
        }) { IsBackground = true }.Start();
    }
    public void ReqContract(Contract contract) {
        client.reqContractDetails(0, contract);
    }

    public void Print() {
        foreach (var contract in contracts) {
            contract.Print();
        }
    }

    public override void contractDetails(int reqId, ContractDetails contractDetails) {
        Console.Write($"{contractDetails.Contract.ConId, -20}");
        Console.Write($"{contractDetails.Contract.Symbol, -10}");
        Console.Write($"{contractDetails.Contract.Right, -10}");
        Console.Write($"{contractDetails.Contract.SecType}\n");
        contracts.Add(new OptionWithPrice(contractDetails.Contract, this));
    }

    public override void contractDetailsEnd(int reqId) {
        Console.WriteLine("ContractDetailsEnd");
    }

    public override void tickPrice(int tickerId, int field, double price, TickAttrib attribs) {
        if (price <= 0.0) return;
        switch (field) {
            case TickType.ASK:
            case TickType.DELAYED_ASK:
                askPriceEvent.Invoke(tickerId, price);
                break;
            case TickType.BID:
            case TickType.DELAYED_BID:
                bidPriceEvent.Invoke(tickerId, price);
                break;
            default:
                break;
        }
    }

    public override void tickSnapshotEnd(int tickerId) {
        Console.WriteLine($"TickSpanshot end. {tickerId}");
    }

    public override void error(int id, int errorCode, string errorMsg, string advancedOrderRejectJson) {
        switch (errorCode) {
            case 10167:
            case 10197:
            case 10090:
                return;
            case 2158:
                client.reqMarketDataType(3);
                break;
            default:
                break;
        }
        Console.WriteLine($"error: {id}: {errorCode}: {errorMsg}");
    }
}
