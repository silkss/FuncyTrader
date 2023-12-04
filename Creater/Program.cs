using Core.Services.Connectors;
using Core.Types;
using IBApi;
using Services.Repositories.Base;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

internal class Program {
    private static readonly ConnectorSettings settings = new ConnectorSettings() {
        Host = "127.0.0.1",
        Port = 7497,
        ClientId = 1112
    };
    private static readonly CreatorSettings creatorSettings = new("gcg4", "comex", 30, 11, 5, 5, 11);
    private static readonly Creator creator = new(creatorSettings);
    private static readonly CancellationTokenSource cts = new();
    private static readonly IConnector connector = new IbConnector(settings);
    private static readonly Repo<TradeContainer> containers = new();
    private static string appFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IronCondor");
    private static string containersFolder = Path.Combine(appFolder, "IronCondors");
    private static readonly JsonSerializerOptions jsonOptions = new() {
        WriteIndented = true
    };
    private static void createIfNotExist(string folder) {
        if (!Directory.Exists(folder)) {
            Directory.CreateDirectory(folder);
        }
    }

    private static async Task Main(string[] args) {
        //var basisName = args[0];
        //var basisExchange = args[1];
        createIfNotExist(appFolder);
        createIfNotExist(containersFolder);

        foreach(var file in Directory.GetFiles(containersFolder, "*.json")) {
            var container = JsonSerializer.Deserialize<TradeContainer>(File.ReadAllText(file));
            if (container != null) {
                containers.Add(container);
            }
        }

        var token = cts.Token;
        await creator.Creation(connector, containers, token);
        if (connector.Connected) {
            connector.Disconnect();
            await Task.Delay(1000);
        }

        foreach (var container in containers.GetAll()) {
            File.WriteAllText(
                $"{containersFolder}/{container.Basis.Name}.json",
                JsonSerializer.Serialize(container, jsonOptions)
                );
        }
    }
}