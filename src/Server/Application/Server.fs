module Application.Server

open Saturn
open Application.Configurations
open Application.Api

let app (cfg: Configuration) =
    let serverUrl =
        sprintf "http://0.0.0.0:%d" cfg.Service.Port

    application {
        pipe_through (Pipelines.api cfg)
        error_handler Handlers.errorHandler
        use_router Routers.api
        url serverUrl
        use_config (fun _ -> cfg)
        service_config (Services.configure cfg)
        use_json_settings Json.settings
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
