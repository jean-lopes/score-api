namespace Domain.Repositories

open Domain.Entities
open System.Threading.Tasks

type IScoreRepository =
    abstract insert : score: Score -> Task<Result<int, exn>>
    abstract findByCpf : cpf: string -> Task<Result<Score option, exn>>
