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

module Results =
    // TODO would be nice to allow only types deriving from Score entity
    // such that we can have ScoreResult<Score> and ScoreResult<Score option>
    type ScoreResult<'a> =
        | Success of 'a
        | InvalidCpf of string
        | Unexpected of exn

    type CreateScoreResult =
        | Success of Score
        | InvalidCpf of string
        | Unexpected of exn

    type FindScoreResult =
        | Success of Score
        | NotFound
        | InvalidCpf of string
        | Unexpected of exn
