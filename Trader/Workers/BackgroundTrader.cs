using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Trader.Services;

namespace Trader.Workers;

public class BackgroundTrader : BackgroundService {
    private readonly ILogger<BackgroundTrader> logger;
    private readonly IConnector connector;

    public BackgroundTrader(ILogger<BackgroundTrader> logger, IConnector connector) {
        this.logger = logger;
        this.connector = connector;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            if (!connector.IsConnected) {
                logger.LogInformation("Connector not connected at: {time}", DateTimeOffset.Now);
                connector.Connect();
                await Task.Delay(5000, stoppingToken);
                
                continue;
            }
            logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(5000, stoppingToken);
        }
    }
}
