# Sourcies allocation

## Exercise definition

The task is to create a mechanism for allocating resources named "Sourcies" which may be requested by multiple clients.  
The requests need to be aggregated and requested from an external API, which we *do not control*, that returns a list of approved Sourcies.  
Approved Sourcies need to be distributed to the clients that requested them, in as fair a way as possible, considering that the approved Sourcies **may not fully cover** the requested amount!  
(Though the definition of "fair" is part of this exercise too!)

The requested Sourcies are received from a CSV file of the following format:  

```csv
Client,Symbol,Quantity
Client1,ABC,300
Client2,QQQ,100
Client2,ABC,200
Client3,TTT,100
```

(As for headers, you can assume whichever is easier to deal with, just make sure to note in solution's note whether it's assumed headers exist or not).

Notice that each combination of client and symbol may only appear once.  
Also, quantity is always a multiple of 100.  
(What to do when a combination appears more than once, or quantity is not a multiple of 100 are part of the exercise as well).

It is known the allocation service API has the following signature:  
`dict[str, int] -> dict[str, int]`  
where both requested quantities and allocated result are maps from Symbols to an integer quantity. The server is **intentionally** unaware of the requesting clients.  
(Technically, since F# is a very strongly typed programming language, with ample support for clever domain modeling, current implementation actually is `Map<Symbol, Request list> -> Map<Symbol, int>`, where requests are defined as `type Request = {Cliet: Client, Quantity: int}` and the client type is simply an enumerator of known clients).

As noted previously, the approved result might return only a subset of the requested Sourcies, in extreme not approving any Sourcies for a given symbol at all!

For example, when requesting:  
`{“SymbolA” : 500, “SymbolB” : 400, “SymbolC”: 700}`  
it may return:  
`{“SymbolA” : 500, “SymbolB” : 345}`  
(Notice `SymbolC` is *entirely* missing, having no sources approved to it at all, and `SymbolB` getting only part of what was requested).

Given the constraint of what was actually approved by the allocation service, our service is tasked with distributing the Sourcies in as fair a way as possible between the requesting clients, under the following guidelines:

1. If only part of the request was approved, the distribution of the results should be proportional to the requested sum.
2. Since clients use the Sourcies in chunks of 100, any chunk not divisible by 100 is less useful. So, for example, giving 200 to one client and 110 to another is better than giving 180 to the first and 130 to the second (even if the second distribution is more “fair”) since it provides 3 “round” chunks of 100.
3. All approved Sourcies should be distributed.
4. No client should receive more than requested.

## Discussion

This exercise requires the solver to tap into several characteristics: a QA, for finding the edge cases, a PM for deciding on how to resolve those edge cases (including defining what a "fair" distribution actually is), and, lastly, a developer for implementing the solution.  
What is called in the tech industry "a usual Monday" 😄.

The most major issue the solver needs to tackle in this exercise is defining a fair distribution when a symbol *does* get an allocation though not enough to cover all requests.  
(The case where a symbol simply didn't receive any allocation is trivial - no one is getting nothing. Done.  
And, of course, the happiest path is if the approved amount fully covers the request, then everybody gets what they asked for.  
A case where approval exceeds demand is **an error** in the approval service and needs to be **clearly** indicated).

In such cases the guidelines instruct us to allocate proportionally to the request pool, i.e., if, for a given symbol, client1 asks for 400 units, client2 asks for 100 units, for a total request of 500 units, but only 300 units are approved, then the allocation should be 80% (i.e., 4 parts out of total 5) to client1 for a batch of 240, and the other 60 to client2.

But, that contradicts the guideline instructing that batches of multiples of 100 are better than "actual" fair distribution, so maybe in this case a distribution of 200-100 is better? Maybe even 300-0?  
There's no "right" answer.  
The solver, donning the PM hat needs to come up with an answer and justify it.

But there is an **even bigger** issue with the guidelines' definition of fairness, i.e., proportionality!

Assume one, or some, of the clients get wind that the distribution engine uses this method.  
What would be their logical response?  
Game theory has us covered with an answer: "The tragedy of the commons".  
All clients privy to our little distribution engine secret will pump up their requested amount, asking for much more than they actually need, such that when the pie is split, their proportion is relatively large and they get more source allocated to them!

Now, if we, as owners of the distribution service notice this behavior we can punish those clients.  
But what if we disregard the proportionality guideline and come up with a better scheme?

The fairest distribution would be to chuck all approved Sourices to batches of 100, the de-facto useful amount, then round-robin the chunks between all those who requested it.  
The leftover amount? We can decide ad-hoc, by an algorithm... So many ways to decide, but the majority of the approved resource **will** be distributed fairly in a round-robin.

Also, if we want to gift, or punish, a client, round-robin is easiest to implement such a behavior: simply, when its the client's turn allocate them 2 chunks, or none, or whatever.  
Round-robin makes **controlling** the distribution easier. And smarter.

Another question that arises is what to do when the unique combination of client and symbol shows up in the requests CSV more than once.

This contradicts the guideline, but what should we do when such a predicament strikes (and it *will* strike)?

I can see two approaches:

1. Ignore any subsequent lines. Consider them a mistake.
2. Aggregate their quantity, thus having a single line per each client/symbol combo, having the total amount of all the times the combo showed up in the CSV file.

In my solution I opted for the second option, though I can see merit in the first as well, namely: make sure the data you pass out is correct.  
(Much like my service does some defensive coding to make sure the data it receives is correct, not trusting unknown inputs.  
Quality is a team effort and each service owner should make sure their service output is correct, just like they validate the inputs to their service for correctness before feeding it as input to the actual API!)

Again, which option to choose, or maybe there's some other method, is where the solver of the exercise shows their PM abilities.

Some more issues that may arise are what to do if the amount requested by a client, in the CSV file is not a multiple of 100.  
That is an error according to the guidelines.  
Again, more of a PM decision than anything else, in my solution I ignore such entries as mistakes, not adding them to the total amount requested by the client (for that specific symbol, of course).

As a matter of decision, I've also implemented the approver service to approve only multiples of 10.  
This was **not** in the guideline or the spec, it is a call I made because I like nice, whole, round numbers.  
(Real-life approver service is free to do its thing. Anyway the allocation algorithm will simply toss all the leftover, not multiple of 100, according to its own logic. That was *purely* a stylistic choice on my account!)

I couldn't find any other edge cases, or unique conditions that require decision making, but I enjoyed the possibility to exercise the PM in me (professionally I'm a QA, so making calls is not in my purview).

## Running

This solution is a `dotnet` one, implemented in the `F#` programming language.

In order to run it:

1. Make sure to have `dotnet` runtime installed.
   1. This solution targets `dotnet` 7 and above!
   2. If you want to tinker with the solution you'll need `dotnet` 7 (or above) SDK, not just runtime.
2. Clone the solution to your machine then, in the solution's folder, run `dotnet tool install`.
3. Penultimate step, run `dotnet restore`.
4. Lastly, run `dotnet run`.
