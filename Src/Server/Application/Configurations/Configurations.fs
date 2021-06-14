namespace Application.Configurations

open System.IO
open Commons.Extensions
open System

[<AutoOpen>]
module Configurations =
    type Database =
        { Host: string
          Port: int
          User: string
          Password: string
          Name: string }

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


    let private loadFromFilesAndEnv (paths: seq<string>) =
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

    /// Load variables from files and system environment.
    ///
    /// Load order: paths (by sequence order), then Environment Variables
    ///
    /// Last read value is kept as the current value
    let buildFromFilesAndEnv paths =
        let envs = loadFromFilesAndEnv paths
        let getStr name = getStrFromMap name envs
        let getInt name = getIntFromMap name envs
        let getBool name = getBoolFromMap name envs

        { Service =
              { Port = getInt VariableNames.Service.port
                Secret = getStr VariableNames.Service.secret
                Key = getStr VariableNames.Service.key
                UnauthorizedAsNotFound = getBool VariableNames.Service.unauthorizedAsNotFound
                ScoreBounds =
                    let min =
                        getInt VariableNames.Service.minScoreBound

                    let max =
                        getInt VariableNames.Service.maxScoreBound

                    new ScoreBounds(min, max) }
          Database =
              { Host = getStr VariableNames.Database.host
                Port = getInt VariableNames.Database.port
                User = getStr VariableNames.Database.user
                Password = getStr VariableNames.Database.password
                Name = getStr VariableNames.Database.name } }
