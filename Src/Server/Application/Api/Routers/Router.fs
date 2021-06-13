namespace Application.Api.Routers

open Saturn
open Giraffe.ResponseWriters

[<AutoOpen>]
module Router =
    let appRouter cfg =
        router {
            //TODO json
            not_found_handler (text "API endpoint not found")
            get "/scores" (json """{"hello": "world"}""")
            get "/abc" (text (invalidOp "dsaldsa"))
        }
