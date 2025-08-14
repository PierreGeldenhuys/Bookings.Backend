# Bookings.Backend
## Requirement
The system you are working on is used to make bookings for people going on holiday, this could be
apartments, vehicles, shows etc. The assumption we work on is that there will always be availability of
what needs to be booked. You do not need any persistence to SQL / file etc. this can be stored in
memory or wherever you prefer. You are part of the backend engineering team and, the requirement is
for a basic solution, that will allow someone to add a booking, edit a booking or delete a booking. If you
can add a unit test or frontend (console, web etc.) to show it working it would be great.

## Stack
dotnet, C#

## Tooling
pwsh 7, k6, dotnet-counters

## Build
`dotnet build`

## Run
`dotnet run --project Bookings.Api`

## Test
`dotnet test`

## Test Coverage
`.\TestCoverage.ps1`
`.\TestCoverage.ps1 -Open`
### [view report](https://pierregeldenhuys.github.io/Bookings.Backend/)

## Load Test
`.\LoadTest.ps1`
(only black box)
### [view concurrency edit load test](https://pierregeldenhuys.github.io/Bookings.Backend/Concurrent_Edit_Test.html)
### [view baseline load test](https://pierregeldenhuys.github.io/Bookings.Backend/Baseline_Load_Test.html)
### [view spike load test](https://pierregeldenhuys.github.io/Bookings.Backend/Spike_Load_Test.html)
### [view mixed crud load test](https://pierregeldenhuys.github.io/Bookings.Backend/Mixed_CRUD_Load_Test.html)

## Memory Profile
`.\ProfileMemory.ps1` then analyze `.\counters.csv`
(conducted during load test)

Working set memory (dotnet.process.memory.working_set) is consistently high
around 150 MB, which is expected for a minimal API under load � nothing alarming.

Heap segment breakdown (gc.heap.size subsegments):
gc.heap.gen0.size, gen1.size, gen2.size � all show typical short-lived object collection behavior.
Spikes in Gen 0 are expected under request bursts.

No sudden spikes or memory leaks:
No increase in heap or working set over time.
Memory looks stable and reclaimed periodically.

## Retrospective and Approach
I test drove this initially but later switched to a hybrid approach, sometimes writing the method before the test. 
I defined my tests prior to writing the code and this kept me test focused either way.
I was aiming for 90% test coverage.

I focused on clean and simplistic concurrency-safe datatypes available in dotnet C# for the in-mem store, instead of struggling with manual locking code, which is tedious to maintain.
I learned about record structs while trying to make the lookups for create and update idempotency checks o(1) instead of o(n).

I wanted to demonstrate load testing as far left as possible so I opted for k6 as tooling and consulted GPT 4o-mini on what kind of load scenarios this kind of application can expect and wrote the tests around that.

## Path to Production
My next steps would have been to 
- run a resource usage trace while performing the load testing and check for leaks (dotnet-trace)
- dockerize the app and k6, update the readme with the related instructions and cli commands
- load test with docker in the mix (whiter box)
- setup k8 deployment and vertical scaling initially, update the readme with the related instructions and cli commands
- load test with k8 in the mix (whiter box)
- play around with [pulumi](https://www.pulumi.com/) and build an azure dev env with C# (AKS, App Insights, Az Monitor, Az Redis, Az KeyVault)
- build the deployment pipeline
- automate load testing in dev env post deploy
- see how hard I can push this with load while vertical scaling, thinking about thresholds and cloud bills
- introduce redis as store and horisontal scaling
- see how hard I can push this with load again
- build the prod env etc etc