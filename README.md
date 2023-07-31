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

It is known the approval service API has the following pseudo-signature:  
`(Map from symbol to quantity) -> (Map symbol to quantity)`  
where both requested quantities and approved result are maps/dicts/hashes (depending on your programming language of choice) from symbols to **integer** quantities.  
The server is **intentionally** unaware of the requesting clients.  

As noted previously, the approved result might return only a subset of the requested Sourcies, in extreme not approving any Sourcies for a given symbol at all!

For example, when requesting:  
`{“SymbolA” : 500, “SymbolB” : 400, “SymbolC”: 700}`  
it may return:  
`{“SymbolA” : 500, “SymbolB” : 345}`  
(Notice `SymbolC` is *entirely* missing, having no resources approved to it at all, and `SymbolB` getting only part of what was requested).

Given the constraint of what was actually approved by the approval service, our service is tasked with distributing the Sourcies in as fair a way as possible between the requesting clients, under the following guidelines:

1. If only part of the request was approved, the distribution of the results should be proportional to the requested sum.
2. Since clients use the Sourcies in chunks of 100, any chunk not divisible by 100 is less useful.  
 So, for example, giving 200 to one client and 110 to another is better than giving 180 to the first and 130 to the second (even if the second distribution is more “fair”) since it provides 3 “round” chunks of 100.
3. All approved Sourcies should be distributed.
4. No client should receive more than requested.

## Discussion

This exercise requires the solver to tap into several characteristics: a QA, for finding the edge cases, a PM for deciding on how to resolve those edge cases (including defining what a "fair" distribution actually is), and, lastly, a developer for implementing the solution.  
What is called in the tech industry "a usual Monday" 😄.

### Distribution fairness

The most major issue the solver needs to tackle in this exercise is defining a fair distribution when a symbol receives only partial approval, only some, not all, of the requests can be completed, if at all.  
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
All clients privy to our little distribution engine's secret will pump up their requested amount, asking for much more than they actually need, such that when the pie is split, their take is relatively large and they get more resources allocated to them!

Now, if we, as owners of the distribution service notice this behavior we can punish those clients, or we could reward faithful clients.  
Either way, the stick or the carrot, requires us to monitor the resource requests and try to see if a pattern of abuse emerges.

So, is there a better distribution scheme to try?

The fairest distribution would be to chuck all approved Sourices to batches of 100, the de-facto useful amount, then round-robin the chunks between all those who requested it with the leftover being distributed according to its own, "surplus logic".  
In this manner we can also punish, or reward, selected clients easily: when it's the specific client's turn to be allocated resources, we can manipulate its allocation the way we see fit!  
Round-robin makes **controlling** the distribution easier and smarter.

### Malformed requests CSV file

Another question that arises is what to do when the unique combination of client and symbol shows up in the requests CSV more than once.

This contradicts the guideline, but what should we do when such a predicament strikes (and it *will* strike)?

I can see two approaches:

1. Ignore any subsequent lines. Consider them a mistake.
2. Aggregate their quantity, thus having a single line per each client/symbol combo, having the total amount of all the times the combo showed up in the CSV file.

In my solution I opted for the second option, though I can see merit in the first as well, namely: make sure the data you pass out is correct.

Again, which option to choose, or maybe there's some other method, is where the solver of the exercise shows their PM abilities.

Some more issues that may arise are what to do if the amount requested by a client, in the CSV file, is not a multiple of 100.  
That is an error according to the guidelines.  
Again, more of a PM decision than anything else, in my solution I ignore such entries as mistakes, not adding them to the total amount requested by the client (for that specific symbol, of course).

### Personal preference

A stylistic decision I've made was to implement the approver service to approve only multiples of 10.  
This was **not** in the guideline or the spec, it is a call I made because I like nice, whole, round numbers.
