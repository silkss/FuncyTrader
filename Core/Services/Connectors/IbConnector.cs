using IBApi;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Core.Types;
using Core.Services.Converters;

namespace Core.Services.Connectors;

public class ConnectorSettings {
    public required string Host { get; set; }
    public required int Port { get; set; }
    public required int ClientId { get; set; }
}

public class IbConnector : DefaultEWrapper, IConnector {
    private class InstrumentQueueItem(int reqId, Instrument? instrument) {
        public int ReqId { get; } = reqId;
        public Instrument? Instrument { get; } = instrument;
    }

    private class OptionChainQueueItem {
        public OptionChainQueueItem(int reqId, List<OptionChain> options) {
            ReqId = reqId;
            Options = options;
        }

        public readonly int ReqId;
        public readonly List<OptionChain> Options;
    }

    private readonly Queue<OptionChainQueueItem> optionChains = new();
    private readonly Queue<InstrumentQueueItem> instruments = new();
    private  ConnectorSettings? settings;
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

    public event PriceEventHandler LastPriceEvent = delegate { };

    public IbConnector(ConnectorSettings? settings) {
        this.settings = settings;
        monitor = new EReaderMonitorSignal();
        client = new EClientSocket(this, monitor);
    }

    #region IConnector
    public bool Connected { get => client.IsConnected(); }

    public Task<Instrument?> GetFuture(string name, string exchange) {
        var contract = new Contract() {
            LocalSymbol = name.Trim().ToUpper(),
            Exchange = exchange.Trim().ToUpper(),
            SecType = "FUT"
        };
        return requestInstrument(contract);
    }
    public Task<Instrument?> GetPut(Instrument parent, string tradingClass, double strike, DateTime expiration) {
        var contract = new Contract() {
            Strike = strike,
            LastTradeDateOrContractMonth = expiration.ToString("yyyyMMdd"),
            Exchange = parent.Exchange,
            Symbol  = parent.Symbol,
            TradingClass = tradingClass,
            SecType = "FOP",
            Right = "P"
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
    public void Disconnect() => client.eDisconnect();
    public void RequestMarketData(Instrument instrument) {
        client.reqMktData(instrument.Id, instrument.ToIbContract(), string.Empty, false, false, null);
    }
    public Task<IEnumerable<OptionChain>> GetOptionChains(Instrument instrument) {
        if (instrument.Type != InstrumentType.Future) throw new NotSupportedException($"Requestin options for type {instrument.Type} not supported");

        return Task<IEnumerable<OptionChain>>.Factory.StartNew(() => {
            IEnumerable<OptionChain>? options;
            lock (optionChains) {
                client.reqSecDefOptParams(instrument.Id, instrument.Symbol, instrument.Exchange, "FUT", instrument.Id);
                while (!optionChains.TryPeek(out var ocqi) && ocqi?.ReqId != instrument.Id) {
                    Monitor.Wait(optionChains);
                }
                options = optionChains.Dequeue().Options;
            }
            return options;
        });
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
        var instrument = contractDetails.ToInstrument();
        var iqi = new InstrumentQueueItem(reqId, instrument);
        lock (instruments) {
            instruments.Enqueue(iqi);
            Monitor.PulseAll(instruments);
        }
    }
    public override void securityDefinitionOptionParameter(int reqId, string exchange, int underlyingConId, string tradingClass, string multiplier, HashSet<string> expirations, HashSet<double> strikes) {
        foreach (var expiration in expirations) {
            var optionChain = new OptionChain(tradingClass, exchange, expiration.ToDateTime(), strikes);
            lock (optionChains) {
                if (optionChains.TryPeek(out var ocqi)) {
                    ocqi.Options.Add(optionChain);
                } else {
                    var options = new List<OptionChain> {
                        optionChain
                    };

                    var newOCQI = new OptionChainQueueItem(reqId, options);
                    optionChains.Enqueue(newOCQI);
                }
            }
        }
    }
    public override void securityDefinitionOptionParameterEnd(int reqId) {
        lock (optionChains) {
            if (!optionChains.TryPeek(out var ocqi) && ocqi?.ReqId != reqId) {
                optionChains.Enqueue(new OptionChainQueueItem(reqId, []));
            }
            Monitor.PulseAll(optionChains);
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
                client.reqMarketDataType(3);
                break;
            default:
                break;
        }
        Console.WriteLine($"Ошибка: {id}: {errorCode}: {errorMsg}");
    }
    public override void error(string str) => Console.WriteLine($"Ошибка: {str}");
    #endregion
}
