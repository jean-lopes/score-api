namespace Application.Configurations

open System
open System.IO
open Npgsql.FSharp
open Commons.Extensions

[<AutoOpen>]
module Configurations =
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

    let private strVarToTuple (s: string) =
        match s.Split("=") with
        | [| name; value |] -> Some(name, value)
        | _ -> None

    let private loadFromFile (path: string) =
        File.ReadAllLines(path)
        |> Seq.cast<string>
        |> Seq.choose strVarToTuple
        |> Map.ofSeq

    let private loadFromFiles (paths: seq<string>) =
        paths
        |> Seq.filter File.Exists
        |> Seq.map loadFromFile
        |> Seq.reduce (fun acc e -> Map.merge acc e)

    let private getEnv name =
        match Environment.GetEnvironmentVariable name with
        | null -> None
        | value -> Some(name, value)

    let private loadEnvs =
        VariableNames.asSeq
        |> Seq.choose getEnv
        |> Map.ofSeq

    let private loadFromFilesAndEnv (paths: seq<string>) : Map<string, string> =
        let fromEnv = loadEnvs
        let fromFiles = loadFromFiles paths
        Map.merge fromFiles fromEnv

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

    let private loadDatabaseFromMap (map: Map<string, string>) : Database =
        let reader = ConfigurationReader(map)

        { ConnectionString =
              Sql.host (reader.GetStr VariableNames.Database.host)
              |> Sql.database (reader.GetStr VariableNames.Database.name)
              |> Sql.username (reader.GetStr VariableNames.Database.user)
              |> Sql.password (reader.GetStr VariableNames.Database.password)
              |> Sql.port (reader.GetInt VariableNames.Database.port)
              |> Sql.formatConnectionString }

    /// Load variables from files and system environment.
    ///
    /// Load order: paths (by sequence order), then Environment Variables
    ///
    /// Last read value is kept as the current value
    let buildDatabaseConfig (paths: seq<string>) : Database =
        let envs = loadFromFilesAndEnv paths
        loadDatabaseFromMap envs

    /// Load variables from files and system environment.
    ///
    /// Load order: paths (by sequence order), then Environment Variables
    ///
    /// Last read value is kept as the current value
    let buildFromFilesAndEnv paths =
        let envs = loadFromFilesAndEnv paths
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
          Database = loadDatabaseFromMap envs }
