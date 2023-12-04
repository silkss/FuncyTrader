using ConsoleTrader.Types;
using IBApi;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ConsoleTrader.Services.Connectors;

internal class ConnectorSettings {
    public required string Host { get; set; }
    public required int Port { get; set; }
    public required int ClientId { get; set; }
}

internal class Connector : DefaultEWrapper, IConnector {
    private class InstrumentQueueItem(int reqId, Instrument? instrument) {
        public int ReqId { get; } = reqId;
        public Instrument? Instrument { get; } = instrument;
    }
    
    private Queue<InstrumentQueueItem> instruments = new();
    private ConnectorSettings? settings;
    private readonly EClientSocket client;
    private readonly EReaderMonitorSignal monitor;
    private int instrumentReqId = 0;

    private Task<Instrument?> requestInstrument(Contract contract) {
        var id = instrumentReqId++;
        client.reqContractDetails(id, contract);

        return Task<Instrument?>.Factory.StartNew(() => {
            Instrument? instrument;
            lock (instruments) {
                while (!instruments.TryPeek(out var iqi) && iqi?.ReqId != id) {
                    Monitor.Wait(instruments);
                }
                instrument = instruments.Dequeue().Instrument;
            }
            return instrument;
        });
    }
    private void connect() {
        client.eConnect(settings!.Host, settings!.Port, settings!.ClientId);
        var reader = new EReader(client, monitor).Start();
        new Thread(() => {
            while (client.IsConnected()) {
                monitor.waitForSignal();
                reader.processMsgs();
            }
        }) { IsBackground = true }.Start();
    }

    public event Action ConnectEvent = delegate { };
    public event PriceEventHandler LastPriceEvent = delegate { };

    public Connector(ConnectorSettings? settings) {
        this.settings = settings;
        monitor = new EReaderMonitorSignal();
        client = new EClientSocket(this, monitor);
    }

    #region IConnector
    public Task<Instrument?> GetFuture(string name, string exchange) {
        var contract = new Contract() {
            LocalSymbol = name,
            Exchange = exchange,
            SecType = "FUT"
        };
        return requestInstrument(contract);
    }

    public void Connect() {
        if (this.settings == null) {
            Console.WriteLine("Не могу подключиться. Нет настроек");
            return;
        }
        connect();
    }
    #endregion

    #region EWrapper
    public override void tickPrice(int tickerId, int field, double price, TickAttrib attribs) {
        if (price <= 0.0) return;
        if (price == Double.MaxValue) return;
        switch (field) {
            case TickType.LAST:
            case TickType.DELAYED_LAST:
                LastPriceEvent.Invoke(tickerId, (decimal)price);
                break;
            default:
                break;
        }
    }
    public override void contractDetails(int reqId, ContractDetails contractDetails) {
        var instrument = Instrument.Create(contractDetails);
        var iqi = new InstrumentQueueItem(reqId, instrument);
        lock (instruments) {
            instruments.Enqueue(iqi);
            Monitor.PulseAll(instruments);
        }
    }
    public override void error(Exception e) => Console.WriteLine(e.Message);
    public override void error(int id, int errorCode, string errorMsg, string advancedOrderRejectJson) {
        switch (errorCode) {
            case 200:
                var iqi = new InstrumentQueueItem(id, null);
                lock (instruments) {
                    instruments.Enqueue(iqi);
                    Monitor.PulseAll(instruments);
                }
                break;
            case 2158:
                ConnectEvent.Invoke();
                break;
            default:
                break;
        }
        Console.WriteLine($"Ошибка: {id}: {errorCode}: {errorMsg}");
    }
    public override void error(string str) => Console.WriteLine($"Ошибка: {str}");
    #endregion
}
