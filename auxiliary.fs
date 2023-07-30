namespace SourciesDistribute

type Symbol =
    | ABB
    | CDD
    | EFF
    | GHH

type Client =
    | C1
    | C2
    | C3
    | C4
    | C5
    | C6
    | C7

type Request = { Client: Client; Quantity: int }

[<AutoOpen>]
module Auxiliary =
    let symbolTotalRequestSum (sourciesRequested: Map<Symbol, Request list>) =
        sourciesRequested
        |> Map.map (fun _ s -> List.sumBy (fun curRqst -> curRqst.Quantity) s)
