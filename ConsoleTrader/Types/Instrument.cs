using IBApi;
using System;
using System.Globalization;

namespace ConsoleTrader.Types;

internal record Instrument {
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string Symbol { get; init; }
    public required string Exchange { get; init; }
    public required DateTime Expiration { get; init; }

    internal static Instrument Create(ContractDetails contract) {
        return new Instrument() {
            Id = contract.Contract.ConId,
            Expiration = DateTime.ParseExact(
                contract.Contract.LastTradeDateOrContractMonth,
                "yyyyMMdd",
                CultureInfo.InvariantCulture),
            Name = contract.Contract.LocalSymbol,
            Symbol = contract.Contract.Symbol,
            Exchange = contract.Contract.Exchange,
        };
    }
}
