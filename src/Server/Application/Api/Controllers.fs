namespace Application.Api

module ScoresController =
    open Saturn
    open FSharp.Control.Tasks
    open System.Threading.Tasks
    open Microsoft.AspNetCore.Http
    open Application.Api.Dtos
    open Domain.Services
    open Giraffe.Core

    let showAction (ctx: HttpContext) (cpf: string) =
        task {
            let service = ctx.GetService<ScoreService>()

            let! result = service.getByCpf cpf

            match result with
            | Ok (Some score) -> return! Response.ok ctx score
            | Ok None -> return! Response.notFound ctx ()
            | Error ex -> return raise ex
        }

    let createAction (ctx: HttpContext) =
        task {
            let service = ctx.GetService<ScoreService>()

            let! body = Controller.getJson<Requests.Score> ctx

            let! result = service.create body.cpf

            match result with
            | Ok score -> return Response.created ctx score
            | Error ex -> return raise ex
        }

    let resource =
        controller {
            create createAction
            show showAction
        }
