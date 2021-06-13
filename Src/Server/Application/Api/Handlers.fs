namespace Application.Api

module Handlers =
    open System.Threading.Tasks
    open FSharp.Control.Tasks
    open Microsoft.AspNetCore.Http
    open Giraffe.Core

    let empty (status: int) (ctx: HttpContext) : Task<HttpContext option> =
        ctx.SetStatusCode(status)
        task { return Some ctx }
