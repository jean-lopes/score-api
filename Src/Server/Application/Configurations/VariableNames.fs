namespace Application.Configurations

module VariableNames =
    [<RequireQualifiedAccessAttribute>]
    module Service =
        let port = "SERVICE_PORT"
        let secret = "SERVICE_SECRET"
        let key = "SERVICE_KEY"

        let asSeq =
            seq {
                port
                secret
                key
            }

    let variableNamesAsSeq = Service.asSeq
