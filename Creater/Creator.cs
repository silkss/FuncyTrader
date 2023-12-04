using Core.Services.Connectors;
using Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Services.Repositories.Base;

public enum CreatorStatus { NeededBasis, NeededOptionChains, NeededOptionStrategies, Done, Error }

public record CreatorSettings(
    string BasisName,
    string BasisExchange, 
    int DaysToExpiration,
    int LowBuySteps, int LowSellSteps,
    int UpSellSteps, int UpBuySteps);

internal class Creator(CreatorSettings settings) {
    private readonly CreatorSettings settings = settings;

    public CreatorStatus Status { get; private set; }
    public Instrument? Basis { get; private set; }
    public decimal LastPrice { get; private set; }
    private void lastPriceHandler(int tickerId, decimal price) {
        if (tickerId != Basis?.Id) return;
        LastPrice = price;
    }

    private async void setBasis(IConnector connector) {
        Console.WriteLine("Requesting basis");
        Basis = await connector.GetFuture(settings.BasisName, settings.BasisExchange);
        if (Basis != null) {
            connector.LastPriceEvent += lastPriceHandler;
            connector.RequestMarketData(Basis);
            Status = CreatorStatus.NeededOptionChains;
        } else {
            Status = CreatorStatus.Error;
            Error = "Basis is null";
        }
    }
    private async void setOptionChains(IConnector connector) {
        Console.WriteLine("Requesting option chain");
        if (Basis == null) {
            setError("Basis is null. Cant request option chains!");
            return;
        }

        if (OptionChains != null && OptionChains.Any()) {
            Status = CreatorStatus.NeededOptionStrategies;
            return;
        }

        OptionChains = await connector.GetOptionChains(Basis);
        if (OptionChains == null) {
            setError("Option chain is null");
            return;
        }

        if (!OptionChains.Any()) {
            setError("Option chain is empty!");
            return;
        }

        Status = CreatorStatus.NeededOptionStrategies;
    }
    private async void creatingOptionStrategy(IConnector connector, Repo<TradeContainer> containers) {
        Console.WriteLine("Starting startegy creation");
        if (Basis == null) {
            setError("Basis is null. Cant request option chains!");
            return;
        }

        if (OptionChains != null && !OptionChains.Any()) {
            Status = CreatorStatus.NeededOptionStrategies;
            return;
        }
        
        if (OptionChains == null) {
            setError("Option chain is null");
            return;
        }

        if (LastPrice == 0.0m) {
            Console.WriteLine("Waiting for basis last price");
            return;
        }

        var tradingClass = OptionChains.FirstOrDefault(oc => (oc.expiration - DateTime.Now).Days >= settings.DaysToExpiration);
        if (tradingClass == null) {
            setError("cant choose option trading class for cretion");
            return;
        }

        var orderedStrikes = tradingClass.Strikes.OrderBy(s => s).ToList();
        var closestStrike = orderedStrikes.MinBy(s => Math.Abs(s - (double)LastPrice));
        var closestStrikeId = orderedStrikes.FindIndex(s => s == closestStrike);
        
        if (closestStrikeId == 0) {
            setError("Cant find closest strike");
            return;
        }

        var low_buy_put = await connector.GetPut(
            Basis, 
            tradingClass.TradingClass, 
            orderedStrikes[closestStrikeId - settings.LowBuySteps], 
            tradingClass.expiration);

        var low_sell_put = await connector.GetPut(
            Basis, 
            tradingClass.TradingClass,
            orderedStrikes[closestStrikeId - settings.LowSellSteps],
            tradingClass.expiration);

        var up_sell_put = await connector.GetPut(
            Basis, 
            tradingClass.TradingClass, 
            orderedStrikes[closestStrikeId + settings.UpBuySteps],
            tradingClass.expiration);

        var up_buy_put = await connector.GetPut(
            Basis, 
            tradingClass.TradingClass, 
            orderedStrikes[closestStrikeId + settings.UpBuySteps],
            tradingClass.expiration);

        if (low_buy_put == null) {
            setError("Low buy is null!");
            return;
        }
        if (low_sell_put == null) {
            setError("Low sell is null");
            return;
        }

        if (up_sell_put == null) {
            setError("Up sell is null.");
            return;
        }
        if (up_buy_put == null) {
            setError("up buy is null.");
            return;
        }

        var ironCondor = new IronCondorStrategy() {
            LowBuy = low_buy_put,
            LowSell = low_sell_put,
            UpSell = up_sell_put,
            UpBuy = up_sell_put
        };

        var container = containers.GetAll().FirstOrDefault(c => c.Basis == Basis);
        if (container == null) {
            container = new TradeContainer() { Basis = Basis };
            container.IronCondors.Add(ironCondor);
            containers.Add(container);
        } else {
            container.IronCondors.Add(ironCondor);
        }
        Status = CreatorStatus.Done;
    }

    private void setError(string errorMsg) {
        Status = CreatorStatus.Error;
        Error = errorMsg;
    }

    public IEnumerable<OptionChain>? OptionChains { get; private set; }
    public async Task Creation(IConnector connector, Repo<TradeContainer> containers, CancellationToken token) {
        var working = true;
        while (!token.IsCancellationRequested && working) {
            await Task.Delay(2000, token);
            if (!connector.Connected) {
                connector.Connect();
                continue;
            }

            switch (Status) {
                case CreatorStatus.NeededBasis:
                    setBasis(connector);
                    break;
                case CreatorStatus.NeededOptionChains when Basis != null:
                    setOptionChains(connector);
                    break;
                case CreatorStatus.NeededOptionStrategies:
                    creatingOptionStrategy(connector, containers);
                    break;
                case CreatorStatus.Done:
                    Console.WriteLine("DONE");
                    working = false;
                    break;
                case CreatorStatus.Error:
                    Console.WriteLine($"ERROR: {Error}");
                    working = false;
                    break;
                default:
                    break;
            }
        }
    }
    public string? Error { get; private set; }
}
