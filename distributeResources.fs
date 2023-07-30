namespace SourciesDistribute

open System

[<AutoOpen>]
module DistributeSourcies =
    let distributeSourcies (requestMap: Map<Symbol, Request list>) (approvedMap: Map<Symbol, int>) =
        approvedMap
        |> Map.iter (fun curSym curApproved ->
            let requestedQuant = (symbolTotalRequestSum requestMap)[curSym]

            match (curApproved, requestedQuant) with
            | x, y when x > y ->
                raise
                <| ArgumentException
                    $"\nApproved quantity for symbol %A{curSym}, %i{curApproved}, is greater than demand, %i{requestedQuant}. There is an error in approval service. Aborting as data is considered useless."
            | x, y when x = y ->
                printfn $"\nApproved quantity for symbol %A{curSym} meets demand exactly.\nOptimal allocation follows:"

                requestMap[curSym]
                |> List.iter (fun curRqst -> printfn $"Client: %A{curRqst.Client}, Quantity: %i{curRqst.Quantity}")
            | x, y when x < y ->
                printfn
                    $"\nApproved quantity for symbol %A{curSym}, %i{curApproved} is **less** than demand, %i{requestedQuant}.\nApproved resources will be dispensed in batches of 100, round-robin. Surplus will be dispensed to next in line.\nNeeds to implement this soon."
            | _ ->
                raise
                <| ArgumentException "No idea how we got to this branch! Need to do some serious debugging.")
