namespace Application.Api

[<RequireQualifiedAccessAttribute>]
module Pipelines =
    open Saturn
    open Application.Api.Authorization
    open Application.Configurations

    let api (cfg: Configuration) =
        pipeline {
            plug acceptJson
            plug requestId
            plug (requiresAuthorization cfg cfg.Service.Secret)
        }
