using System;
using System.Collections.Generic;

namespace Core.Types;
public record OptionChain(string TradingClass, string Exchange, DateTime expiration, HashSet<double> Strikes);
