
namespace Trader.Services.IB;
public class IbSettings {
    public IbSettings(string host, int port, int clientId) {
        Host = host;
        Port = port;
        ClientId = clientId;
    }

    public string Host { get; }
    public int Port { get; }
    public int ClientId { get; }
}
