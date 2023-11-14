using Trader.Services.IB;

namespace Trader.Services;
public interface ISettings {
    IbSettings? GetIbSettings();
}

public class Settings : ISettings {
    private readonly IbSettings ibSettings;

    public Settings() {
        this.ibSettings = new IbSettings("127.0.0.1", 7497, 1002);
    }

    public Settings(IbSettings ibSettings) {
        this.ibSettings = ibSettings;
    }

    public IbSettings? GetIbSettings() => ibSettings;
}
