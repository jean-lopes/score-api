namespace Domain.Services

open System
open Domain.Entities.Score
open Domain.Validators

type IScoreProvider =
    abstract score : cpf: string -> int

type ScoreService(scoreProvider: IScoreProvider) =
    member _.create(cpf: string) : Result<Score, exn> =
        let c = cpf.Trim()

        match Cpf.validate c with
        | Some err -> Error(invalidArg "cpf" err)
        | None ->
            Ok
                { Id = Guid.NewGuid()
                  Cpf = cpf.Trim()
                  Value = scoreProvider.score cpf
                  CreatedAt = DateTime.UtcNow }

    member _.getByCpf(cpf: string) : Result<Score option, exn> =
        Ok
        <| Some
            { Id = Guid.NewGuid()
              Cpf = cpf
              Value = scoreProvider.score cpf
              CreatedAt = DateTime.UtcNow }
