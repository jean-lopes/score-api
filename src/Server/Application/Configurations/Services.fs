namespace Application.Configurations

open System
open Microsoft.Extensions.DependencyInjection
open Domain.Services
open Resources.Repositories
open Resources.Services

[<RequireQualifiedAccessAttribute>]
module Services =
    let configure (cfg: Configuration) (services: IServiceCollection) =
        let scoreBounds = cfg.Service.ScoreBounds

        let scoreProvider =
            RandomScoreProvider(Random(), scoreBounds.Min, scoreBounds.Max)

        let scoreRepository =
            PgSqlScoreRepository(cfg.Database.ConnectionString) // InMemoryScoreRepository()

        let scoreService =
            ScoreService(scoreProvider, scoreRepository)

        services.AddSingleton<ScoreService>(scoreService)
