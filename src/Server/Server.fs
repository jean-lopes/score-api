module Server

open Saturn
open Config
open Application

let apiRouter =
    router {
        not_found_handler (fun _ -> Handlers.empty 404)
        forward "/scores" ScoresController.resource
    }

let apiPipeline cfg =
    pipeline {
        plug acceptJson
        plug requestId
        plug (requiresAuthorization cfg cfg.Service.Secret)
    }

let app (cfg: Configuration) =
    let serverUrl =
        sprintf "http://0.0.0.0:%d" cfg.Service.Port

    application {
        pipe_through (apiPipeline cfg)
        error_handler Handlers.errorHandler
        use_router apiRouter
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
