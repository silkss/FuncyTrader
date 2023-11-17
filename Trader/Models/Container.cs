using Trader.Services.Connectors;

namespace Trader.Models;

public class Container {
    private void futureEventHandler(int reqId, IConnector connector, Instrument future) {
        if (reqId != Id) return;
        Basis ??= future;
        connector.RequestMarketData(Basis);
        connector.FutureEvent -= futureEventHandler;
    }
    private void requestBasis(IConnector connector) {
        connector.FutureEvent += futureEventHandler;
        connector.RequestFuture(Id, Settings.Symbol, Settings.Exchange, Settings.Expiration);
    }

    public int Id;
    public required ContainerSettings Settings { get; init; }
    public Instrument? Basis { get; set; }

    public void Work(IConnector connector) {
        if (Basis == null) {
            requestBasis(connector);
            return;
        }
    }
}
