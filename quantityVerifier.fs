namespace SourciesDistribute

open System

(*
This module take the "Fail fast" approach, raising every single check the data fails on, instead of error handling using either `Option` or `Result` types.
The reason is the data passed into the verifier function is supposed to already have been vetted out by prior services, **mainly the data generator service itself**.
If the data is wrong in some way, the generator is malfunctioning and we need to fix the error at its root: the generator service.
No point in handling a **prior** error **here** and delegating the error further down the pipeline.
If something is broken - fix it! Don't cover it up and hope for the best!
*)

[<AutoOpen>]
module QuantityVerifier =
    let verifyQuantity (globalUserInput: int) =
        let validateNonNegative (intUserInput: int) =
            if (intUserInput >= 0) then
                intUserInput
            else
                raise
                <| ArgumentException
                    $"Invalid user input, %i{intUserInput} is a negative value. Ignoring value for current accumulation, adding 0 instead."

        let validateMultiple100 (nonNegUserInput: int) =
            if (nonNegUserInput) % 100 = 0 then
                nonNegUserInput
            else
                raise
                <| ArgumentException
                    $"Invalid user input, %i{nonNegUserInput}, is not a \"clean\" multiple of 100. Ignoring value for current accumulation, adding 0 instead."

        validateNonNegative globalUserInput |> validateMultiple100
