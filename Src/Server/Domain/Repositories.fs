namespace Domain.Repositories

open Domain.Entities

type IScoreRepository =
    abstract insert (score: Score) : unit = ()
