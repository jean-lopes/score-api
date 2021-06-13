namespace Application.Configurations

open System.IO
open Commons.Extensions
open Application.Configurations.VariableNames
open System

[<AutoOpen>]
module Configuration =
    type Service =
        { Port: int
          Secret: string
          Key: string }

    type Configuration = { Service: Service }

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
        variableNamesAsSeq
        |> Seq.choose getEnv
        |> Map.ofSeq


    let private loadFromFilesAndEnv (paths: seq<string>) =
        let fromEnv = loadEnvs
        let fromFiles = loadFromFiles paths
        Map.merge fromFiles fromEnv

    let private getStrFromMap name (map: Map<string, string>) =
        Map.tryFind name map
        |> fun x ->
            match x with
            | Some value -> value
            | None -> failwithf "Expected variable `%s`" name

    let private getIntFromMap name map =
        getStrFromMap name map
        |> fun s ->
            match Int32.TryParse s with
            | true, n -> n
            | _ -> failwithf "Expected variable `%s` to be an int, found: `%s`" name s

    /// Load variables from files and system environment.
    ///
    /// Load order: paths (by sequence order), then Environment Variables
    ///
    /// Last read value is kept as the current value
    let buildFromPathAndEnv paths =
        let envs = loadFromFilesAndEnv paths
        let getStr name = getStrFromMap name envs
        let getInt name = getIntFromMap name envs

        { Service =
              { Port = getInt Service.port
                Secret = getStr Service.secret
                Key = getStr Service.key } }
