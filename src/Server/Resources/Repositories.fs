namespace Resources.Repositories

open Domain.Entities
open Domain.Repositories
open System.Threading.Tasks
open FSharp.Control.Tasks.ContextInsensitive

[<AutoOpen>]
module ScoreRepositories =

    type InMemoryScoreRepository() =
        let mutable scores : Map<string, Score> = Map.empty

        interface IScoreRepository with
            member _.insert(score: Score) : Task<Result<int, exn>> =
                task {
                    scores <- scores.Add(score.Cpf, score)
                    return (Ok 1)
                }

            member _.findByCpf(cpf: string) : Task<Result<Score option, exn>> =
                task {
                    return
                        match scores.TryFind cpf with
                        | Some s -> Ok(Some s)
                        | None -> Ok None
                }
