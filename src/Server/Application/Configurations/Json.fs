namespace Application.Configurations

open Newtonsoft.Json
open Newtonsoft.Json.Serialization

[<RequireQualifiedAccessAttribute>]
module Json =
    let settings =
        let s = JsonSerializerSettings()
        let resolver = DefaultContractResolver()
        resolver.NamingStrategy <- SnakeCaseNamingStrategy()
        s.ContractResolver <- resolver
        s
