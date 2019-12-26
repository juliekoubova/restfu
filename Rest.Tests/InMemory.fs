module Rest.Tests.InMemoryTests
open Expecto
open Rest


type Thingy = {
  Id : string
}

let withInMemoryResource f () =
  let res =
    InMemory.Create (fun e -> e.Id)
    |> RestApiExplorer.applyEntityKeyName
  f res

[<Tests>]
let tests =
  testList "InMemory" [
    yield! testFixture withInMemoryResource [

      "Gets an existing entity", (fun res ->
        let entity = { Id = "42" }

        Expect.equal
          (Post entity |> res.Handler)
          (PostSuccess (Created, ("42", Some entity)))
          "Unexpected POST result"

        Expect.equal
          (Get "42" |> res.Handler)
          (GetSuccess (Ok, ("42", entity)))
          "Unexpected GET result"
      )

      "Creates new Entity", (fun res ->
        let entity = { Id = "42" }
        Expect.equal
          (Post entity |> res.Handler)
          (PostSuccess (Created, ("42", Some entity)))
          "Unexpected result"
      )

      "Doesn't create an Entity with existing Id", (fun res ->
        let entity = { Id = "42" }

        Expect.equal
          (Post entity |> res.Handler)
          (PostSuccess (Created, ("42", Some entity)))
          "First create result unexpected"

        Expect.equal
          (Post entity |> res.Handler)
          (PostFail ({
            Status = Conflict
            Title ="Thingy with the specified Id already exists."
            Description ="Thingy with Id \"42\" already exists."
            Type = "https://github.com/juliekoubova/tired-rest/wiki/Problems#already-exists"
          }, entity))
          "Second create result unexpected"
      )
    ]
  ]
