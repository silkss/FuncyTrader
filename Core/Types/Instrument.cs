using System;

namespace Core.Types;
public enum InstrumentType { Future, Call, Put }
public record Instrument {
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string Symbol { get; init; }
    public required string Exchange { get; init; }
    public required DateTime Expiration { get; init; }
    public double Strike { get; init; }
    public required InstrumentType Type { get; init; }
}
