namespace Infrastructure

open System
open System.IO
open System.Threading.Tasks
open System.Security.Cryptography
open System.Text
open FSharp.Control.Tasks.ContextInsensitive
open Npgsql.FSharp
open Domain
open Domain.Repositories
open Domain.Services

type Encryption(key: byte array) =
    member _.encrypt(value: string) =
        use aes = Aes.Create()
        aes.Key <- key
        aes.IV <- key

        printfn "%s" (Convert.ToBase64String(aes.Key))

        let valueBytes = ASCIIEncoding.UTF8.GetBytes(value)

        use encryptor = aes.CreateEncryptor(aes.Key, aes.IV)

        let encrypted =
            encryptor.TransformFinalBlock(valueBytes, 0, valueBytes.Length)

        Convert.ToBase64String(encrypted)


    member _.decrypt(encrypted: string) : string =
        use aes = Aes.Create()
        aes.Key <- key
        aes.IV <- key

        let value = Convert.FromBase64String(encrypted)

        use decryptor = aes.CreateDecryptor(aes.Key, aes.IV)

        let result =
            decryptor.TransformFinalBlock(value, 0, value.Length)

        ASCIIEncoding.UTF8.GetString(result)


type RandomScoreProvider(rnd: Random, min: int, max: int) =
    interface IScoreProvider with
        member _.score _ = rnd.Next(min, max + 1)

type InMemoryScoreRepository() =
    let mutable scores : Map<uint64, Score> = Map.empty

    interface IScoreRepository with
        member _.insert(score: Score) : Task<Result<int, exn>> =
            task {
                scores <- scores.Add(score.Cpf.Raw, score)
                return (Ok 1)
            }

        member _.findByCpf(cpf: CPF) : Task<Result<Score option, exn>> =
            task { return Ok(scores.TryFind cpf.Raw) }

type PgSqlScoreRepository(connectionString: string, encryption: Encryption) =
    let insertAsync score =
        connectionString
        |> Sql.connect
        |> Sql.query "INSERT INTO scores (id, cpf, value, created_at) VALUES(@id, @cpf, @value, @created_at)"
        |> Sql.parameters [ "@id", Sql.uuid score.Id
                            "@cpf", Sql.string (encryption.encrypt (score.Cpf.Raw.ToString()))
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

    let findByCpfAsync (cpf: CPF) =
        connectionString
        |> Sql.connect
        |> Sql.query findByCpfQuery
        |> Sql.parameters [ "@cpf", Sql.string (encryption.encrypt (cpf.Raw.ToString())) ]
        |> Sql.executeAsync
            (fun read ->
                { Id = read.uuid "id"
                  Cpf = redacted (uint64 (encryption.decrypt (read.string "cpf")))
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
