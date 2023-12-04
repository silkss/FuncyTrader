using ConsoleTrader.Types;
using System;
using System.Threading.Tasks;

namespace ConsoleTrader.Services.Connectors;

internal delegate void PriceEventHandler(int tickerId, decimal price);
internal interface IConnector {
    event Action ConnectEvent;
    event PriceEventHandler LastPriceEvent;

    void Connect();
    Task<Instrument?> GetFuture(string name, string exchange);
}