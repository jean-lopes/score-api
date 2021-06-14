namespace Application.Api

module ScoresController =
    open FSharp.Control.Tasks
    open Microsoft.AspNetCore.Http
    open Saturn
    open Giraffe.Core
    open Application.Api.Dtos
    open Domain.Services

    let showAction (ctx: HttpContext) (cpf: string) =
        task {
            let service = ctx.GetService<ScoreService>()

            let! result = service.getByCpf cpf

            match result with
            | Ok (Some score) -> return! Response.ok ctx (scoreResponse score)
            | Ok None -> return! Response.notFound ctx ()
            | Error ex -> return raise ex
        }

    let createAction (ctx: HttpContext) =
        task {
            let service = ctx.GetService<ScoreService>()

            let! body = Controller.getJson<ScoreRequest> ctx

            let! result = service.create body.cpf

            match result with
            | Ok score -> return Response.created ctx (scoreResponse score)
            | Error ex -> return raise ex
        }

    let resource =
        controller {
            create createAction
            show showAction
        }
