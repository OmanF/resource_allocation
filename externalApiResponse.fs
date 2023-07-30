namespace SourciesDistribute

open System

[<AutoOpen>]
module ExternalApiResponse =
    let rnd = new Random()

    let approveSourcies (sourciesRequested: Map<Symbol, Request list>) =
        sourciesRequested
        |> symbolTotalRequestSum
        |> Map.map (fun _ s ->
            // With probability of 1-in-4, allocation for symbol might not cover the entire requested amount!
            if rnd.NextDouble() >= 0.75 then
                // Round allocation **down** to nearest multiple of 10
                (((((float s * rnd.NextDouble()) / 10.0) |> ceil) * 10.0) |> Math.Round) |> int
            else
                s)
