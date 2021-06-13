module Application.Api.Authorization

open Giraffe.Core
open Giraffe.ResponseWriters
open Microsoft.AspNetCore.Http
open System.Threading.Tasks
open Application.Api.Entities.Error

let private unauthorized =
    fun (ctx: HttpContext) ->
        ctx.SetStatusCode(401)
        ctx.WriteJsonAsync { message = "Unauthorized" }

let requiresAuthorization (value: string) : HttpHandler =
    fun nxt (ctx: HttpContext) ->
        match ctx.Request.Headers.TryGetValue "Authorization" with
        | false, _ -> unauthorized ctx
        | true, v ->
            if v.Equals(value) then
                nxt ctx
            else
                unauthorized ctx
