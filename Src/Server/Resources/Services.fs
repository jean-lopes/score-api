namespace Resources.Services

open Domain.Services

[<AutoOpen>]
module ScoreProviders =
    open System

    type RandomScoreProvider(rnd: Random, min: int, max: int) =
        interface IScoreProvider with
            member _.score _ = rnd.Next(min, max + 1)
