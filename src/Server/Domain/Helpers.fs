namespace Domain.Helpers

[<RequireQualifiedAccessAttribute>]
module Cpf =
    open System
    open System.Text.RegularExpressions
    open Domain.Entities

    [<Literal>]
    let private cpfPattern = @"^(\d{11}|\d{3}\.\d{3}\.\d{3}\-\d{2})$"

    let format (cpf: CPF) : string = cpf.ToString("000\.000\.000\-00")

    let tryParse (rawCpf: string) : Result<CPF, string> =
        let digits : string -> string = String.filter (fun c -> Char.IsDigit c)

        let cpf = rawCpf.Trim()

        if Regex.IsMatch(cpf, cpfPattern) then
            Ok(uint64 <| digits cpf)
        else
            Error "Invalid CPF"
