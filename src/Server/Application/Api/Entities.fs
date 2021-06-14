namespace Application.Api

module Dtos =
    type Error = { message: string; cause: exn }

    [<RequireQualifiedAccessAttribute>]
    module Requests =
        type Score = { cpf: string }
