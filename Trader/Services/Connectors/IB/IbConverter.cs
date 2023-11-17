using IBApi;
using System;
using System.Globalization;
using Trader.Models;

namespace Trader.Services.Connectors.IB;

internal static class IbConverter {
    public static string ToIbDateTime(this DateOnly date) {
        return date.ToString("yyyyMMdd");
    }
    public static DateOnly ToDateOnly(this string ibDate) {
        return DateOnly.ParseExact(ibDate, "yyyyMMdd", CultureInfo.InvariantCulture);
    }
    public static Instrument ToInstrument(this ContractDetails contract) {
        return new Instrument() {
            Exhcnage = contract.Contract.Exchange,
            Expiration = contract.Contract.LastTradeDateOrContractMonth.ToDateOnly(),
            Id = contract.Contract.ConId,
            Name = contract.Contract.LocalSymbol
        };
    }
    public static Contract ToContract(this Instrument instrument) {
        return new Contract() {
            ConId = instrument.Id,
            LocalSymbol = instrument.Name,
            Exchange = instrument.Exhcnage,
            LastTradeDateOrContractMonth = instrument.Expiration.ToIbDateTime()
        };
    }

}
