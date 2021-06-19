namespace Config

open System
open System.Text
open System.Security.Cryptography
open Microsoft.Extensions.DependencyInjection
open Npgsql.FSharp
open Domain.Services
open Infrastructure

type Database = { ConnectionString: string }

[<Struct>]
type ScoreBounds =
    val Min: int
    val Max: int

    new(min, max) =
        { Min = min; Max = max }
        then
            if max < min then
                failwithf "Invalid score bounds, max < min. Min: %d, Max: %d" min max

type Service =
    { Port: int
      Secret: string
      Key: string
      UnauthorizedAsNotFound: bool
      ScoreBounds: ScoreBounds }

type Configuration =
    { Service: Service
      Database: Database }

[<RequireQualifiedAccessAttribute>]
module private VariableNames =
    [<RequireQualifiedAccessAttribute>]
    module Database =
        let host = "POSTGRES_HOST"
        let port = "POSTGRES_PORT"
        let user = "POSTGRES_USER"
        let password = "POSTGRES_PASSWORD"
        let name = "POSTGRES_DB"

        let asSeq =
            seq {
                host
                port
                user
                password
                name
            }

    [<RequireQualifiedAccessAttribute>]
    module Service =
        let port = "SERVICE_PORT"
        let secret = "SERVICE_SECRET"
        let key = "SERVICE_KEY"
        let unauthorizedAsNotFound = "SERVICE_UNAUTHORIZED_AS_NOT_FOUND"
        let minScoreBound = "SERVICE_SCORE_BOUND_MIN"
        let maxScoreBound = "SERVICE_SCORE_BOUND_MAX"

        let asSeq =
            seq {
                port
                secret
                key
                unauthorizedAsNotFound
                minScoreBound
                maxScoreBound
            }

    let asSeq = Seq.append Service.asSeq Database.asSeq

[<RequireQualifiedAccess>]
module Configurations =
    let private getEnv name =
        match Environment.GetEnvironmentVariable name with
        | null -> None
        | value -> Some(name, value)

    let private loadEnvs =
        VariableNames.asSeq
        |> Seq.choose getEnv
        |> Map.ofSeq

    let private getStrFromMap name (map: Map<string, string>) : string =
        Map.tryFind name map
        |> fun x ->
            match x with
            | Some value -> value
            | None -> failwithf "Expected variable `%s`" name

    let private getIntFromMap name map : int =
        getStrFromMap name map
        |> fun s ->
            match Int32.TryParse s with
            | true, n -> n
            | _ -> failwithf "Expected variable `%s` to be an int, found: `%s`" name s

    let private getBoolFromMap name map : bool =
        getStrFromMap name map
        |> fun s ->
            match bool.TryParse s with
            | true, value -> value
            | _ -> failwithf "Expected variable `%s` to be a bool, found: `%s`" name s

    type private ConfigurationReader(variables: Map<string, string>) =
        member _.GetStr(name: string) = getStrFromMap name variables
        member _.GetInt(name: string) = getIntFromMap name variables
        member _.GetBool(name: string) = getBoolFromMap name variables

    let fromEnv =
        let envs = loadEnvs
        let reader = ConfigurationReader(envs)

        { Service =
              { Port = reader.GetInt VariableNames.Service.port
                Secret = reader.GetStr VariableNames.Service.secret
                Key = reader.GetStr VariableNames.Service.key
                UnauthorizedAsNotFound = reader.GetBool VariableNames.Service.unauthorizedAsNotFound
                ScoreBounds =
                    let min =
                        reader.GetInt VariableNames.Service.minScoreBound

                    let max =
                        reader.GetInt VariableNames.Service.maxScoreBound

                    new ScoreBounds(min, max) }
          Database =
              { ConnectionString =
                    Sql.host (reader.GetStr VariableNames.Database.host)
                    |> Sql.database (reader.GetStr VariableNames.Database.name)
                    |> Sql.username (reader.GetStr VariableNames.Database.user)
                    |> Sql.password (reader.GetStr VariableNames.Database.password)
                    |> Sql.port (reader.GetInt VariableNames.Database.port)
                    |> Sql.formatConnectionString } }

[<RequireQualifiedAccessAttribute>]
module Json =
    open Newtonsoft.Json
    open Newtonsoft.Json.Serialization

    let settings =
        let s = JsonSerializerSettings()
        let resolver = DefaultContractResolver()
        resolver.NamingStrategy <- SnakeCaseNamingStrategy()
        s.ContractResolver <- resolver
        s

[<RequireQualifiedAccessAttribute>]
module Services =
    let encryption cfg =
        use md5 = MD5.Create()

        let keyBytes =
            ASCIIEncoding.UTF8.GetBytes(cfg.Service.Key)

        let hashedKey = md5.ComputeHash(keyBytes)

        Encryption(hashedKey)

    let configure (cfg: Configuration) (services: IServiceCollection) =
        printfn "configure"
        let scoreBounds = cfg.Service.ScoreBounds

        let scoreProvider =
            RandomScoreProvider(Random(), scoreBounds.Min, scoreBounds.Max)

        let scoreRepository =
            PgSqlScoreRepository(cfg.Database.ConnectionString, encryption cfg)

        let scoreService =
            ScoreService(scoreProvider, scoreRepository)

        services.AddSingleton<ScoreService>(scoreService)
