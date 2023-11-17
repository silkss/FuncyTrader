using System;

namespace Trader.Models;

public class ContainerSettings {
    public required string Symbol { get; init; }
    public required string Exchange { get; init; }
    public required DateOnly Expiration { get; init; }
}
