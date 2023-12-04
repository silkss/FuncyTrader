using Core.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Services.Connectors;

public delegate void PriceEventHandler(int tickerId, decimal price);
public interface IConnector {
    event PriceEventHandler LastPriceEvent;

    bool Connected { get; }
    void Connect();
    void Disconnect();
    Task<Instrument?> GetFuture(string name, string exchange);
    Task<Instrument?> GetPut(Instrument parent, string tradingClass, double strike, DateTime expiration);
    Task<IEnumerable<OptionChain>> GetOptionChains(Instrument instrument);
    void RequestMarketData(Instrument instrument);
}