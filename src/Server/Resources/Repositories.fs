namespace Resources.Repositories

open Domain.Entities
open Domain.Repositories
open System.Threading.Tasks
open FSharp.Control.Tasks.ContextInsensitive
open Npgsql.FSharp

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

    type PgSqlScoreRepository(connectionString: string) =
        let insertAsync score =
            connectionString
            |> Sql.connect
            |> Sql.query "INSERT INTO scores (id, cpf, value, created_at) VALUES(@id, @cpf, @value, @created_at)"
            |> Sql.parameters [ "@id", Sql.uuid score.Id
                                "@cpf", Sql.int64 (int64 score.Cpf)
                                "@value", Sql.int score.Value
                                "@created_at", Sql.timestamptz score.CreatedAt ]
            |> Sql.executeNonQueryAsync

        [<Literal>]
        let findByCpfQuery = """
            SELECT id
                 , cpf
                 , value
                 , created_at
              FROM scores
             WHERE cpf = @cpf
          ORDER BY created_at DESC
             LIMIT 1
        """

        let findByCpfAsync cpf =
            connectionString
            |> Sql.connect
            |> Sql.query findByCpfQuery
            |> Sql.parameters [ "@cpf", Sql.int64 (int64 cpf) ]
            |> Sql.executeAsync
                (fun read ->
                    { Id = read.uuid "id"
                      Cpf = uint64 (read.int64 "cpf")
                      Value = read.int "value"
                      CreatedAt = read.dateTime "created_at" })

        interface IScoreRepository with
            member _.insert(score: Score) : Task<Result<int, exn>> =
                task {
                    try
                        let! rows = insertAsync score
                        return Ok rows
                    with ex -> return Error ex
                }

            member _.findByCpf(cpf: CPF) : Task<Result<Score option, exn>> =
                task {
                    try
                        let! scores = findByCpfAsync cpf

                        if scores.IsEmpty then
                            return Ok None
                        else
                            return Ok(Some scores.Head)
                    with ex -> return Error ex
                }
