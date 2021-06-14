namespace Domain.Validators

[<RequireQualifiedAccessAttribute>]
module Cpf =
    open System

    let private isDigitOnly : string -> bool = String.forall (fun c -> Char.IsDigit c)

    let validate (cpf: string) : string option =
        let invalid msg = Some(sprintf "%s. CPF: %s" msg cpf)

        match cpf with
        | _ when cpf.Length <> 11 -> invalid "Invalid length. Expected 11"
        | s when isDigitOnly s -> invalid "CPF should contain only numbers"
        | _ -> None
