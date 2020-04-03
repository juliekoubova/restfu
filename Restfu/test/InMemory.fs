module Restfu.Tests.InMemoryTests
open Expecto
open Restfu

let withInMemoryResource f () =
  let res =
    InMemory.Create (fun e -> e.Id)
    |> RestApiExplorer.applyEntityKeyName
  f res

[<Tests>]
let tests =
  testList "InMemory" [
    yield! testFixture withInMemoryResource CrudTests.tests
  ]
