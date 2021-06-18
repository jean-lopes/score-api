namespace Application

open System
open System.Threading.Tasks
open FSharp.Control.Tasks
open Microsoft.AspNetCore.Http
open Newtonsoft.Json
open Giraffe.Core
open Saturn
open Config
open Domain
open Domain.Services

type ErrorResponse = { Message: string; Cause: exn }

type BadRequestResonse = { Message: string }

type ScoreRequest = { Cpf: string }

type ScoreResponse = { Score: int; CreatedAt: DateTime }

[<RequireQualifiedAccessAttribute>]
module Handlers =
    let empty (status: int) (ctx: HttpContext) : Task<HttpContext option> =
        ctx.SetStatusCode(status)
        task { return Some ctx }

    let errorHandler ex _ =
        pipeline {
            set_status_code 500

            json
                { Message = "Uncaught exception"
                  Cause = ex }
        }

[<AutoOpen>]
module Application =
    let scoreResponse (score: Score) : ScoreResponse =
        { Score = score.Value
          CreatedAt = score.CreatedAt }

    let requiresAuthorization cfg (value: string) : HttpHandler =
        fun nxt (ctx: HttpContext) ->
            let statusCode =
                if cfg.Service.UnauthorizedAsNotFound then
                    404
                else
                    401

            match ctx.Request.Headers.TryGetValue "Authorization" with
            | false, _ -> Handlers.empty statusCode ctx
            | true, v ->
                if v.Equals(value) then
                    nxt ctx
                else
                    Handlers.empty statusCode ctx

module ScoresController =
    let showAction (ctx: HttpContext) (cpf: string) =
        task {
            let service = ctx.GetService<ScoreService>()

            let! result = service.find cpf

            match result with
            | Success (Some score) -> return! Response.ok ctx (scoreResponse score)
            | Success None -> return! Response.notFound ctx ()
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
                | Success score -> return! Response.ok ctx (scoreResponse score)
                | InvalidCpf msg -> return! Response.unprocessableEntity ctx { Message = msg }
                | Unexpected ex -> return raise ex
            with :? JsonReaderException as ex -> return! Response.badRequest ctx { Message = ex.Message }
        }

    let resource =
        controller {
            create createAction
            show showAction
        }
