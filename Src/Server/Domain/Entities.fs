namespace Domain.Entities

[<AutoOpen>]
module Score =
    open System

    [<CLIMutable>]
    type Score =
        { Id: Guid
          Cpf: string
          Value: int
          CreatedAt: DateTime }
