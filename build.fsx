#load ".fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO

let srcPath = "./src/"

let migrationPath =
    Path.combine srcPath "Migration/"
    |> Path.getFullName

let migrationProjectPath =
    Path.combine migrationPath "Migration.fsproj"

let appPath =
    Path.combine srcPath "Server/" |> Path.getFullName

let appProjectPath = Path.combine appPath "Server.fsproj"

Target.create "Clean" ignore

Target.create
    "Restore"
    (fun _ ->
        DotNet.restore id migrationProjectPath
        DotNet.restore id appProjectPath)

Target.create
    "Build"
    (fun _ ->
        DotNet.build id migrationProjectPath
        DotNet.build id appProjectPath)

Target.create
    "Env"
    (fun ps ->
        let envFile =
            Option.defaultValue "envs/local.env" (List.tryHead ps.Context.Arguments)

        let strVarToTuple (s: string) =
            match s.Split('=') with
            | [| name; value |] -> Some(name, value)
            | _ -> None

        let setEnv (k, v) = Environment.setEnvironVar k v

        envFile
        |> Path.getFullName
        |> System.IO.File.ReadAllLines
        |> Array.choose strVarToTuple
        |> Array.iter setEnv)

Target.create
    "Migrate"
    (fun _ ->
        DotNet.exec
            (fun p ->
                { p with
                      WorkingDirectory = migrationPath })
            "run"
            ""
        |> ignore)

Target.create
    "Run"
    (fun _ ->
        DotNet.exec (fun p -> { p with WorkingDirectory = appPath }) "watch" "run"
        |> ignore)

"Clean" ==> "Restore" ==> "Build"

"Clean" ==> "Restore" ==> "Env" ==> "Migrate"

"Clean" ==> "Restore" ==> "Env" ==> "Run"

Target.runOrDefaultWithArguments "Build"
