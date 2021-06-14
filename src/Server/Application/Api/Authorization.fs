module Application.Api.Authorization

open Giraffe.Core
open Microsoft.AspNetCore.Http
open Application.Configurations
open Application.Api

let requiresAuthorization cfg (value: string) : HttpHandler =
    let statusCode =
        if cfg.Service.UnauthorizedAsNotFound then
            404
        else
            401

    fun nxt (ctx: HttpContext) ->
        match ctx.Request.Headers.TryGetValue "Authorization" with
        | false, _ -> Handlers.empty statusCode ctx
        | true, v ->
            if v.Equals(value) then
                nxt ctx
            else
                Handlers.empty statusCode ctx
