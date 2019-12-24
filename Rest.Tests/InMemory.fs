module Rest.Tests.InMemoryTests
open Rest

open FsUnit
open Xunit
open FsUnit.CustomMatchers

type Thingy = {
  Id : string
}

let thingies () =
  InMemory.Create (fun e -> e.Id)
  |> RestApiExplorer.applyEntityKeyName

[<Fact>]
let ``Creates new Entity`` () =
  let res = thingies ()
  let entity = { Id = "42" }

  Post { Id = "42" }
  |> res.Handler
  |> should equal (PostSuccess (Created, ("42", Some entity)))

[<Fact>]
let ``Doesn't create an Entity with existing Id`` () =
  let res = thingies ()
  let entity = { Id = "42" }

  Post entity
  |> res.Handler
  |> should equal (PostSuccess (Created, ("42", Some entity)))

  let expected : RestResult<string, Thingy> = (PostFail ({
    Status = Conflict
    Title ="Thingy with the specified Id already exists."
    Description ="Thingy with Id \"42\" already exists."
    Type = "https://github.com/juliekoubova/tired-rest/wiki/Problems#already-exists"
  }, entity))

  Post entity |> res.Handler |> should equal expected

[<Fact>]
let ``Gets an existing Entity`` () =
  let res = thingies ()
  let entity = { Id = "42" }

  Post { Id = "42" }
  |> res.Handler
  |> should equal (PostSuccess (Created, ("42", Some entity)))

  Get "42"
  |> res.Handler
  |> should equal (GetSuccess (Ok, ("42", entity)))
