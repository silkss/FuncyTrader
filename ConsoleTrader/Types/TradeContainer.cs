using ConsoleTrader.Services.Connectors;

namespace ConsoleTrader.Types;
internal class TradeContainer {
    private void lastPriceEventHandler(int ticker, decimal price) {
        if (ticker != Basis.Id) return;
        LastPrice = price;
    }

    public decimal LastPrice;
    public required Instrument Basis { get; init; }

    public void RegisterOwnHandlers(IConnector connector) {

    }
}
