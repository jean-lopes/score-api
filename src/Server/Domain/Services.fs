namespace Domain.Services

open System
open System.Threading.Tasks
open FSharp.Control.Tasks.ContextInsensitive
open Domain.Entities
open Domain.Entities.Results
open Domain.Helpers
open Domain.Repositories

type IScoreProvider =
    abstract score : cpf: CPF -> int

type ScoreService(scoreProvider: IScoreProvider, repository: IScoreRepository) =
    member _.create(rawCpf: string) : Task<CreateScoreResult> =
        task {
            match Cpf.tryParse rawCpf with
            | Ok cpf ->
                let score =
                    { Id = Guid.NewGuid()
                      Cpf = cpf
                      Value = scoreProvider.score cpf
                      CreatedAt = DateTime.UtcNow }

                let! result = repository.insert score

                return
                    match result with
                    | Ok _ -> CreateScoreResult.Success score
                    | Error ex -> CreateScoreResult.Unexpected ex
            | Error msg -> return CreateScoreResult.InvalidCpf msg
        }

    member _.find(rawCpf: string) : Task<FindScoreResult> =
        task {
            match Cpf.tryParse rawCpf with
            | Ok cpf ->
                let! result = repository.findByCpf cpf

                return
                    match result with
                    | Ok (Some score) -> Success score
                    | Ok None -> NotFound
                    | Error ex -> Unexpected ex
            | Error msg -> return InvalidCpf msg
        }
