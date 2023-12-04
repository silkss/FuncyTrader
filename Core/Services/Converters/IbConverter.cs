using Core.Types;
using IBApi;
using System;
using System.Globalization;
  
namespace Core.Services.Converters;
internal static class IbConverter {
    public static DateTime ToDateTime(this string ibDateString) {
        return DateTime.ParseExact(ibDateString, "yyyyMMdd", CultureInfo.InvariantCulture);
    }

    public static Contract ToIbContract(this Instrument instrument) {
        return new Contract() {
            ConId = instrument.Id,
            Exchange = instrument.Exchange,
            LastTradeDateOrContractMonth = instrument.Expiration.ToString("yyyyMMdd"),
        };
    }

    public static Instrument ToInstrument(this ContractDetails contract) {
        return new Instrument() {
            Exchange = contract.Contract.Exchange,
            Expiration = contract.Contract.LastTradeDateOrContractMonth.ToDateTime(),
            Id = contract.Contract.ConId,
            Name = contract.Contract.LocalSymbol,
            Symbol = contract.Contract.Symbol,
            Strike = contract.Contract.Strike,
            Type = contract.Contract.SecType switch {
                "FUT" => InstrumentType.Future,
                "FOP" => contract.Contract.Right switch {
                    "C" => InstrumentType.Call,
                    "P" => InstrumentType.Put,
                    _ => throw new NotSupportedException($"Unsupported option right {contract.Contract.Right}")
                },
                _ => throw new NotSupportedException($"Unsupported instrument type {contract.Contract.SecType}")
            }
        };
    }
}
