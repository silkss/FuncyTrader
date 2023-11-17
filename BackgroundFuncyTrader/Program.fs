namespace BackgroundFuncyTrader

open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Connectors

module Program =
    let createHostBuilder args =
        Host.CreateDefaultBuilder(args)
            .ConfigureServices(fun hostContext services ->
                { Host="127.0.0.1"; Port = 7497; ClientId = 1100; }
                |> services.AddSingleton<IbSettings>
                |> ignore
                services.AddSingleton<IConnector, IbConnector>() |> ignore
                services.AddHostedService<Worker>() |> ignore )

    [<EntryPoint>]
    let main args =
        createHostBuilder(args).Build().Run()

        0 // exit code