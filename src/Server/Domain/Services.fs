namespace Domain.Services

open System
open System.Threading.Tasks
open FSharp.Control.Tasks.ContextInsensitive
open Domain.Entities
open Domain.Helpers
open Domain.Repositories

type IScoreProvider =
    abstract score : cpf: CPF -> int

type ScoreService(scoreProvider: IScoreProvider, repository: IScoreRepository) =
    member _.create(rawCpf: string) : Task<Result<Score, exn>> =
        task {
            match Cpf.tryParse rawCpf with
            | Error err -> return Error(invalidArg "cpf" err)
            | Ok cpf ->
                let score =
                    { Id = Guid.NewGuid()
                      Cpf = cpf
                      Value = scoreProvider.score cpf
                      CreatedAt = DateTime.UtcNow }

                let! result = repository.insert score

                return
                    match result with
                    | Ok _ -> Ok score
                    | Error ex -> Error ex
        }

    member _.getByCpf(rawCpf: string) : Task<Result<Score option, exn>> =
        task {
            match Cpf.tryParse rawCpf with
            | Error err -> return Error(invalidArg "cpf" err)
            | Ok cpf -> return! repository.findByCpf cpf
        }
