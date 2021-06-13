namespace Application.Api.Controllers

open System
open Saturn
open FSharp.Control.Tasks
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Application.Api.Entities.Requests
open Domain.Entities.Score

module ScoresController =
    let scoreGenerator : Generator = new Generator(Random(), 1, 1001)

    let showAction (ctx: HttpContext) (cpf: string) : Task<Score> =
        task {
            let output = scoreGenerator.nextFor cpf
            return output
        }

    let createAction (ctx: HttpContext) : Task<HttpContext option> =
        task {
            let! input = Controller.getJson<ScoreRequest> ctx
            //TODO validate
            let output = scoreGenerator.nextFor input.cpf

            return! Response.created ctx output
        }

    let resource =
        controller {
            create createAction
            show showAction
        }
