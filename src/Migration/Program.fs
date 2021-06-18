module Program

open System.Reflection
open SimpleMigrations
open SimpleMigrations.DatabaseProvider
open SimpleMigrations.Console
open Npgsql
open Config

[<EntryPoint>]
let main argv =
    let config = Seq.ofArray argv |> buildDatabaseConfig

    use connection =
        new NpgsqlConnection(config.ConnectionString)

    connection.Open()

    let provider = PostgresqlDatabaseProvider(connection)
    let assembly = Assembly.GetExecutingAssembly()
    let migrator = SimpleMigrator(assembly, provider)
    let consoleRunner = ConsoleRunner(migrator)
    consoleRunner.Run([||]) |> ignore
    0
