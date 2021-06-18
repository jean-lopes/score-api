namespace Application.Api

[<AutoOpen>]
module Dtos =
    open System
    open Domain.Entities

    type ErrorResponse = { Message: string; Cause: exn }

    type BadRequestResonse = { Message: string }

    type ScoreRequest = { Cpf: string }

    type ScoreResponse = { Score: int; CreatedAt: DateTime }

    let scoreResponse (score: Score) : ScoreResponse =
        { Score = score.Value
          CreatedAt = score.CreatedAt }
