namespace Application.Configurations

[<RequireQualifiedAccessAttribute>]
module VariableNames =

    [<RequireQualifiedAccessAttribute>]
    module Database =
        let host = "POSTGRES_HOST"
        let port = "POSTGRES_PORT"
        let user = "POSTGRES_USER"
        let password = "POSTGRES_PASSWORD"
        let name = "POSTGRES_DB"

        let asSeq =
            seq {
                host
                port
                user
                password
                name
            }

    [<RequireQualifiedAccessAttribute>]
    module Service =
        let port = "SERVICE_PORT"
        let secret = "SERVICE_SECRET"
        let key = "SERVICE_KEY"
        let unauthorizedAsNotFound = "SERVICE_UNAUTHORIZED_AS_NOT_FOUND"
        let minScore = "SERVICE_SCORE_MIN"
        let maxScore = "SERVICE_SCORE_MAX"

        let asSeq =
            seq {
                port
                secret
                key
                unauthorizedAsNotFound
                minScore
                maxScore
            }

    let asSeq = Seq.append Service.asSeq Database.asSeq
