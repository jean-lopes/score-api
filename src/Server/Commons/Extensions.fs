namespace Commons.Extensions

[<RequireQualifiedAccessAttribute>]
module Map =
    let merge m1 m2 =
        Map.fold (fun s k v -> Map.add k v s) m1 m2
