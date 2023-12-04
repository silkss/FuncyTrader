using System.Collections.Generic;

namespace Core.Types;
public class TradeContainer {
    public required Instrument Basis { get; init; }
    public List<IronCondorStrategy> IronCondors { get; init; } = [];
}
