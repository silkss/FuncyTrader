namespace Core.Types;

public class IronCondorStrategy {
    public required Instrument LowBuy { get; init; }
    public required Instrument LowSell { get; init; }
    public required Instrument UpSell { get; init; }
    public required Instrument UpBuy { get; init; }
}
