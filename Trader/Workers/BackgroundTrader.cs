using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Trader.Models;
using Trader.Services.Repos;
using Trader.Services.Connectors;

namespace Trader.Workers;

public class BackgroundTrader : BackgroundService {
    private readonly ILogger<BackgroundTrader> logger;
    private readonly IConnector connector;
    private readonly IEnumerable<Container> containers;

    public BackgroundTrader(ILogger<BackgroundTrader> logger, IConnector connector, ContainerRepo containers) {
        this.logger = logger;
        this.connector = connector;
        this.containers = containers.GetItems();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            if (!connector.IsConnected) {
                logger.LogInformation("Connector not connected: {time}", DateTimeOffset.Now);
                connector.Connect();
                await Task.Delay(5000, stoppingToken);
                continue;
            }
            logger.LogInformation("Trader running at: {time}", DateTimeOffset.Now);
            await Task.Delay(5000, stoppingToken);
        }
    }
}
