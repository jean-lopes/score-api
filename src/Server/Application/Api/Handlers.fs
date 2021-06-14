namespace Application.Api

module Handlers =
    open System.Threading.Tasks
    open FSharp.Control.Tasks
    open Microsoft.AspNetCore.Http
    open Giraffe.Core
    open Saturn
    open Application.Api.Dtos

    let empty (status: int) (ctx: HttpContext) : Task<HttpContext option> =
        ctx.SetStatusCode(status)
        task { return Some ctx }

    let errorHandler ex _ =
        pipeline {
            set_status_code 500

            json
                { message = "Uncaught exception"
                  cause = ex }
        }
