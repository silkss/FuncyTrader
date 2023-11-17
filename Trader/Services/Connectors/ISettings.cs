using Trader.Services.Connectors.IB;

namespace Trader.Services.Connectors;
public interface ISettings
{
    IbSettings? GetIbSettings();
}

public class Settings : ISettings
{
    private readonly IbSettings ibSettings;

    public Settings()
    {
        ibSettings = new IbSettings("127.0.0.1", 7497, 1002);
    }

    public Settings(IbSettings ibSettings)
    {
        this.ibSettings = ibSettings;
    }

    public IbSettings? GetIbSettings() => ibSettings;
}
