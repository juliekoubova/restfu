module Rest.Tests.InMemoryTests
open Expecto
open Rest

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
