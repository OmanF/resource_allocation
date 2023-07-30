# Sourcies allocation

## Changes from `master`

* F# is a (very) strongly typed language, which lends itself perfectly to complex domain modelling.
* Given that, the approval API's signature is assumed to be:  
`Map<Symbol, Request list> -> Map<Symbol, int>`  
where `Symbol` is a discriminated union of hard typed symbols allowed in our system, and `Request` was defined as `type Request = {Client: Client, Quantity: int}`, i.e. an F# record comprised of a `Client` type, which is a strongly typed discriminated union of clients (a stronger typed enumerator) and the integer quantity.
* The logic remains the same, obviously.
