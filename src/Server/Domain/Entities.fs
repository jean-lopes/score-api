namespace Domain.Entities

open System

[<AutoOpen>]
module Aliases =
    type CPF = uint64

[<CLIMutable>]
type Score =
    { Id: Guid
      Cpf: CPF
      Value: int
      CreatedAt: DateTime }
