namespace SourciesDistribute

[<AutoOpen>]
module Main =
    [<EntryPoint>]
    let main _ =
        let requests =
            requestsOrder ("/home/ohadfk/locatesDistribution/sampleCsvFiles/sampleGood.csv")

        let approvals = requests |> approveSourcies

        distributeSourcies requests approvals
        0
