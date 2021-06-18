#load ".fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO

let databaseEnvPath = "database.env" |> Path.getFullName

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
        DotNet.restore id migrationProjectPath
        DotNet.build id appProjectPath)

Target.create
    "Migrate"
    (fun _ ->
        Environment.setEnvironVar "TEST_FAKE" "abcd1234"

        DotNet.exec
            (fun p ->
                { p with
                      WorkingDirectory = migrationPath })
            "run"
            databaseEnvPath
        |> ignore)

Target.create
    "Run"
    (fun _ ->
        let server =
            async {
                DotNet.exec (fun p -> { p with WorkingDirectory = appPath }) "watch" "run"
                |> ignore
            }

        [ server ]
        |> Async.Parallel
        |> Async.RunSynchronously
        |> ignore)

"Clean" ==> "Restore" ==> "Build"

"Clean" ==> "Restore" ==> "Run"

Target.runOrDefault "Build"
