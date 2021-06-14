namespace Domain.Services

open System
open System.Threading.Tasks
open FSharp.Control.Tasks.ContextInsensitive
open Domain.Entities.Score
open Domain.Validators
open Domain.Repositories


type IScoreProvider =
    abstract score : cpf: string -> int

type ScoreService(scoreProvider: IScoreProvider, repository: IScoreRepository) =
    member _.create(cpf: string) : Task<Result<Score, exn>> =
        task {
            let c = cpf.Trim()

            match Cpf.validate c with
            | Some err -> return Error(invalidArg "cpf" err)
            | None ->
                let score =
                    { Id = Guid.NewGuid()
                      Cpf = c
                      Value = scoreProvider.score cpf
                      CreatedAt = DateTime.UtcNow }

                repository.insert score |> ignore

                return Ok score
        }

    member _.getByCpf(cpf: string) : Task<Result<Score option, exn>> =
        task {
            let c = cpf.Trim()

            match Cpf.validate c with
            | Some err -> return Error(invalidArg "cpf" err)
            | None -> return! repository.findByCpf c
        }
