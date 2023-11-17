using System;

namespace Trader.Models;
public class Instrument {
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string Exhcnage { get; init; }
    public required DateOnly Expiration { get; init; }
}
