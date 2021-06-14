module Application.Server

open System
open Saturn
open Application.Configurations
open Application.Api
open Microsoft.Extensions.DependencyInjection
open Domain.Services
open Resources.Repositories
open Resources.Services
open Newtonsoft.Json
open Newtonsoft.Json.Serialization

let configureServices (cfg: Configuration) (services: IServiceCollection) =
    let scoreBounds = cfg.Service.ScoreBounds

    let scoreProvider =
        RandomScoreProvider(Random(), scoreBounds.Min, scoreBounds.Max)

    let scoreRepository = InMemoryScoreRepository()

    let scoreService =
        ScoreService(scoreProvider, scoreRepository)

    services.AddSingleton<ScoreService>(scoreService)

let jsonSettings =
    let settings = JsonSerializerSettings()
    let resolver = DefaultContractResolver()
    resolver.NamingStrategy <- SnakeCaseNamingStrategy()
    settings.ContractResolver <- resolver
    settings

let app (cfg: Configuration) =
    let serverUrl =
        sprintf "http://0.0.0.0:%d" cfg.Service.Port

    application {
        pipe_through (Pipelines.api cfg)
        error_handler Handlers.errorHandler
        use_router Routers.api
        url serverUrl
        use_config (fun _ -> cfg)
        service_config (configureServices cfg)
        use_json_settings jsonSettings
    }

[<EntryPoint>]
let main _ =
    printfn "Working directory - %s" (System.IO.Directory.GetCurrentDirectory())

    let paths =
        seq {
            "../../database.env"
            "../../service.env"
        }

    printfn "Loading configuration"
    let cfg = buildFromFilesAndEnv paths

    printfn "Starting server"
    run (app cfg)

    0
