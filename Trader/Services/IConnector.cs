namespace Trader.Services;
public interface IConnector {
    bool IsConnected { get; }
    void Connect();
}
