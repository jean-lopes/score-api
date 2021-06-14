namespace Application.Api

open Saturn
open Application.Api

[<RequireQualifiedAccessAttribute>]
module Routers =
    let api =
        router {
            not_found_handler (fun _ -> Handlers.empty 404)
            forward "/scores" ScoresController.resource
        }
