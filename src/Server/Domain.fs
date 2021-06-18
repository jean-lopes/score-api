namespace Domain

open System
open System.Threading.Tasks
open FSharp.Control.Tasks.ContextInsensitive

[<Struct>]
[<StructuralEquality; StructuralComparison>]
type Redacted<'T>(raw: 'T) =
    member _.Raw = raw
    override _.ToString() = "REDACTED"

and 'T redacted = Redacted<'T>

type CPF = uint64 redacted

type Score =
    { Id: Guid
      Cpf: CPF
      Value: int
      CreatedAt: DateTime }

type ScoreResult<'a> =
    | Success of 'a
    | InvalidCpf of string
    | Unexpected of exn

module private Cpf =
    open System.Text.RegularExpressions

    [<Literal>]
    let cpfPattern = @"^(\d{11}|\d{3}\.\d{3}\.\d{3}\-\d{2})$"

    let tryParse (rawCpf: string) : Result<CPF, string> =
        let digits : string -> string = String.filter (fun c -> Char.IsDigit c)

        let cpf = rawCpf.Trim()

        if Regex.IsMatch(cpf, cpfPattern) then
            Ok(redacted (uint64 <| digits cpf))
        else
            Error "Invalid CPF"

module Repositories =
    type IScoreRepository =
        abstract insert : score: Score -> Task<Result<int, exn>>
        abstract findByCpf : cpf: CPF -> Task<Result<Score option, exn>>

module Services =
    open Repositories

    type IScoreProvider =
        abstract score : cpf: CPF -> int

    type ScoreService(scoreProvider: IScoreProvider, repository: IScoreRepository) =
        member _.create(rawCpf: string) : Task<ScoreResult<Score>> =
            task {
                match Cpf.tryParse rawCpf with
                | Ok cpf ->
                    let score =
                        { Id = Guid.NewGuid()
                          Cpf = cpf
                          Value = scoreProvider.score cpf
                          CreatedAt = DateTime.UtcNow }

                    let! result = repository.insert score

                    return
                        match result with
                        | Ok _ -> Success score
                        | Error ex -> Unexpected ex
                | Error msg -> return InvalidCpf msg
            }

        member _.find(rawCpf: string) : Task<ScoreResult<Score option>> =
            task {
                match Cpf.tryParse rawCpf with
                | Ok cpf ->
                    let! result = repository.findByCpf cpf

                    return
                        match result with
                        | Ok score -> Success score
                        | Error ex -> Unexpected ex
                | Error msg -> return InvalidCpf msg
            }
