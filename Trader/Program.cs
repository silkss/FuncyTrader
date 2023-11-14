using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Trader.Services;
using Trader.Services.IB;
using Trader.Workers;

namespace Trader;

public class Program {
    public static async Task Main(string[] args) {
        await Host.CreateDefaultBuilder(args)
            .ConfigureServices(services => {
                services.AddSingleton<ISettings, Settings>();
                services.AddSingleton<IConnector, IbConnector>();
                services.AddHostedService<BackgroundTrader>();
            })
            .Build()
            .RunAsync();
    }
}