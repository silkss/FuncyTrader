using System;
using System.Collections.Generic;
using System.IO;
using Trader.Models;
using System.Text.Json;
using Microsoft.Extensions.Hosting;

namespace Trader.Services.Repos;

public class ContainerRepo : Repo {
    private static readonly JsonSerializerOptions jsonSerializerOptions = new() {
        WriteIndented = true
    };

    private readonly List<Container> items;

    private void saveAll() {
        foreach (var item in items) {
            var jsonString = JsonSerializer.Serialize(item, jsonSerializerOptions);
            File.WriteAllText($"{folder}\\{item.Id}.json", jsonString);
        }
    }

    private IEnumerable<Container> load() {
        int id = 0;
        foreach (var file in getFilesInFolder(folder)) {
            var text = File.ReadAllText(file);
            var container = JsonSerializer.Deserialize<Container>(text);
            if (container != null) {
                container.Id = id++;
                yield return container;
            }
        }
        yield break;
    }
    public IEnumerable<Container> GetItems() => items;
    public ContainerRepo(IHostApplicationLifetime lifetime) : base(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "trader")) {
        lifetime.ApplicationStopped.Register(saveAll);
        items = new List<Container>(load());
    }
}
