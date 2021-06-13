namespace Domain.Entities

module Score =
    open System

    [<CLIMutable>]
    type Score =
        { Id: Guid
          Cpf: string
          Value: int
          CreatedAt: DateTime }

    [<Struct>]
    type Generator =
        val Rnd: Random
        val MinScore: int
        val MaxScore: int

        new(rnd, min, max) =
            { Rnd = rnd
              MinScore = min
              MaxScore = max }
            then
                if max < min then
                    failwithf "Max score value should be greater or equal to min score value. Min: %d, Max: %d" min max

        member this.nextFor(cpf: string) =
            { Id = Guid.NewGuid()
              Cpf = cpf.Trim()
              Value = this.Rnd.Next(this.MinScore, this.MaxScore + 1)
              CreatedAt = DateTime.UtcNow }
