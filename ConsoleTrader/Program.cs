using ConsoleTrader.Services.Connectors;
using ConsoleTrader.Types;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal class Program {
    static readonly ConnectorSettings settings = new ConnectorSettings() {
        Host = "127.0.0.1",
        Port = 7497,
        ClientId = 1111
    };
    static readonly IConnector connector = new Connector(settings);
    static readonly List<TradeContainer> trades = new List<TradeContainer>();
    static readonly CancellationTokenSource cts = new();

    private static async void connectEventHandler() {
        var future = await connector.GetFuture("G1CZ3", "COMEX");
        if (future == null) {
            Console.WriteLine("Не удалось запросить фьючрс.");
            return;
        }
        var container = new TradeContainer() {
            Basis = future
        };
        trades.Add(container);
    }

    private static void createStrategy(TradeContainer container) {
        if (container.LastPrice == 0.0m) {
            Console.WriteLine("cant create strategy. Becouse last price is 0.0");
            return;
        }
    }

    private static void Main(string[] args) {
        connector.ConnectEvent += connectEventHandler;
        connector.Connect();
        while (!cts.Token.IsCancellationRequested) {
            switch (Console.ReadLine()?.Trim().ToLower()) {
                case "exit":
                    cts.Cancel();
                    break;
                default:
                    break;
            }
        }
    }
}