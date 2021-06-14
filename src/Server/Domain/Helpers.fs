namespace Domain.Helpers

[<RequireQualifiedAccessAttribute>]
module Cpf =
    open System
    open Domain.Entities

    let format (cpf: CPF) : string = cpf.ToString("000\.000\.000\-00")

    let clean (cpf: string) : string =
        String.filter Char.IsDigit cpf
        |> fun s -> s.PadLeft(11, '0')

    let tryParse (cpf: string) : Result<CPF, string> =
        let isDigitOnly : string -> bool = String.forall (fun c -> Char.IsDigit c)
        let invalid msg = Error(sprintf "%s. CPF: %s" msg cpf)

        match clean cpf with
        | s when s.Length <> 11 -> invalid "Invalid length. Expected 11"
        | s when isDigitOnly s |> not -> invalid "CPF should contain only numbers"
        | s -> Ok(uint64 s)
