#load ".fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO

let appPath = "./src/Server/" |> Path.getFullName
let projectPath = Path.combine appPath "Server.fsproj"

Target.create "Clean" ignore

Target.create "Restore" (fun _ -> DotNet.restore id projectPath)

Target.create "Build" (fun _ -> DotNet.build id projectPath)

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
