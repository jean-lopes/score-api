namespace Application.Api

module ScoresController =
    open FSharp.Control.Tasks
    open Microsoft.AspNetCore.Http
    open Newtonsoft.Json
    open Saturn
    open Giraffe.Core
    open Application.Api.Dtos
    open Domain.Entities.Results
    open Domain.Services

    let showAction (ctx: HttpContext) (cpf: string) =
        task {
            let service = ctx.GetService<ScoreService>()

            let! result = service.find cpf

            match result with
            | Success score -> return! Response.ok ctx (scoreResponse score)
            | NotFound -> return! Response.notFound ctx ()
            | InvalidCpf msg -> return! Response.unprocessableEntity ctx { Message = msg }
            | Unexpected ex -> return raise ex
        }

    let createAction (ctx: HttpContext) =
        task {
            try
                let service = ctx.GetService<ScoreService>()

                let! body = Controller.getJson<ScoreRequest> ctx

                let! result = service.create body.Cpf

                match result with
                | CreateScoreResult.Success score -> return! Response.ok ctx (scoreResponse score)
                | CreateScoreResult.InvalidCpf msg -> return! Response.unprocessableEntity ctx { Message = msg }
                | CreateScoreResult.Unexpected ex -> return raise ex
            with :? JsonReaderException as ex -> return! Response.badRequest ctx { Message = ex.Message }
        }

    let resource =
        controller {
            create createAction
            show showAction
        }
