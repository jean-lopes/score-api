module Application.Server

open Saturn
open Application.Api.Routers
open Application.Configurations
open Application.Api.Authorization
open Application.Api.Entities.Error

let apiPipeline cfg =
    pipeline {
        plug acceptJson
        plug requestId
        plug (requiresAuthorization cfg.Service.Secret)
    }

let app (cfg: Configuration) =
    let serverUrl =
        sprintf "http://0.0.0.0:%d" cfg.Service.Port

    application {
        pipe_through (apiPipeline cfg)
        error_handler (fun ex _ -> pipeline { json { message = ex.Message } })
        use_router (appRouter cfg)
        url serverUrl
        use_config (fun _ -> cfg)
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
    let cfg = buildFromPathAndEnv paths

    printfn "Starting server"
    run (app cfg)
    //run app
    0 // return an integer exit code
