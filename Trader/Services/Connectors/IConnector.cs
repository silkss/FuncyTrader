using System;
using Trader.Models;

namespace Trader.Services.Connectors;

public delegate void FutureEventHandler(int reqId, IConnector connector, Instrument future);
public delegate void PriceEventHandler(int tickerId, decimal price);

public interface IConnector {
    event FutureEventHandler FutureEvent;
    event PriceEventHandler LastPriceEvent;

    bool IsConnected { get; }
    void Connect();
    void RequestFuture(int reqId, string symbol, string exchange, DateOnly expiration);
    void RequestMarketData(Instrument instrument);
}
