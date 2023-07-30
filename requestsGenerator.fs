namespace SourciesDistribute

open System
open FSharp.Data

(*
This module take the "Fail fast" approach, raising every single check the data fails on, instead of error handling using either `Option` or `Result` types.
The reason is the data passed into the verifier function is supposed to already have been vetted out by prior services, **mainly the data generator service itself**.
If the data is wrong in some way, the generator is malfunctioning and we need to fix the error at its root: the generator service.
No point in handling a **prior** error **here** and delegating the error further down the pipeline.
If something is broken - fix it! Don't cover it up and hope for the best!
*)

[<AutoOpen>]
module RequestsGenerator =
    type RequestCsvProvider = CsvProvider<"./sampleCsvFiles/sample.csv", HasHeaders=true>

    let requestsMap = Map.empty<Symbol, Request list>

    let translateSymbol (sym: string) =
        match sym.ToLower() with
        | "abb" -> ABB
        | "cdd" -> CDD
        | "eff" -> EFF
        | "ghh" -> GHH
        | _ ->
            raise
            <| ArgumentException($"Unkwown symbol, %s{sym} was given. Please verify your data.")

    let translateClient (client: string) =
        match client.ToLower() with
        | "client1" -> C1
        | "client2" -> C2
        | "client3" -> C3
        | "client4" -> C4
        | "client5" -> C5
        | "client6" -> C6
        | "client7" -> C7
        | _ ->
            raise
            <| ArgumentException($"Unknown client, %s{client} was given. Please verify your data.")

    let requestsOrder (csvFilePath) =
        RequestCsvProvider.Load(csvFilePath: string).Rows
        |> Seq.filter (fun row -> not ((row.Quantity |> verifyQuantity) = 0))
        |> (Seq.groupBy (fun row -> (translateClient row.Client, translateSymbol row.Symbol))
            >> Seq.map (fun (combo, rows) -> (combo, rows |> Seq.sumBy (fun row -> (row.Quantity |> verifyQuantity))))
            >> Seq.fold
                (fun (acc: Map<Symbol, Request list>) (combo, sum) ->
                    if (Map.containsKey (snd combo) acc) then
                        Map.add
                            (snd combo)
                            (List.append acc[snd combo] [ { Client = (fst combo); Quantity = sum } ])
                            acc
                    else
                        Map.add (snd combo) (List.append [] [ { Client = (fst combo); Quantity = sum } ]) acc)
                requestsMap)
