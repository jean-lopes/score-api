namespace Application.Api.Routers

open Saturn
open Application.Api.Controllers
open Application.Api

[<AutoOpen>]
module Router =
    let appRouter cfg =
        router {
            not_found_handler (fun _ -> Handlers.empty 404)
            forward "/scores" ScoresController.resource
        }
