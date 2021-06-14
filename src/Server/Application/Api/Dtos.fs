namespace Application.Api

[<AutoOpen>]
module Dtos =
    open System
    open Domain.Entities

    type Error = { message: string; cause: exn }

    type ScoreRequest = { cpf: string }

    type ScoreResponse = { Score: int; CreatedAt: DateTime }

    let scoreResponse (score: Score) : ScoreResponse =
        { Score = score.Value
          CreatedAt = score.CreatedAt }
