namespace Resources.Repositories

open Domain.Entities
open Domain.Repositories
open System.Threading.Tasks
open FSharp.Control.Tasks.ContextInsensitive

[<AutoOpen>]
module ScoreRepositories =

    type InMemoryScoreRepository() =
        let mutable scores : Map<CPF, Score> = Map.empty

        interface IScoreRepository with
            member _.insert(score: Score) : Task<Result<int, exn>> =
                task {
                    scores <- scores.Add(score.Cpf, score)
                    return (Ok 1)
                }

            member _.findByCpf(cpf: CPF) : Task<Result<Score option, exn>> = task { return Ok(scores.TryFind cpf) }
