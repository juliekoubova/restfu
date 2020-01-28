namespace Rest.Tests
open Expecto
open Rest

[<CLIMutable>]
type Thingy = {
  Id : string
  Value : string
}

module CrudTests =
  let tests =
    [
      let handle res = res.Handler >> Async.RunSynchronously

      "Gets an existing entity", fun res ->
        let entity = { Id = "42"; Value = "Existing" }

        Expect.equal
          (Post entity |> handle res)
          (PostSuccess (Created, ("42", Some entity)))
          "Unexpected POST result"

        Expect.equal
          (Get "42" |> handle res)
          (GetSuccess (Ok, ("42", entity)))
          "Unexpected GET result"


      "Creates new Entity", fun res ->
        let entity = { Id = "42"; Value = "Created" }
        Expect.equal
          (Post entity |> handle res)
          (PostSuccess (Created, ("42", Some entity)))
          "Unexpected result"


      "Doesn't create an Entity with existing Id", fun res ->
        let existing = { Id = "42"; Value = "Existing" }
        let duplicate = { Id = "42"; Value = "Duplicate" }

        Expect.equal
          (Post existing |> handle res)
          (PostSuccess (Created, ("42", Some existing)))
          "First create result unexpected"

        Expect.equal
          (Post duplicate |> handle res)
          (PostFail ({
            Status = Conflict
            Title ="Thingy with the specified Id already exists."
            Description ="Thingy with Id \"42\" already exists."
            Type = "https://github.com/juliekoubova/tired-rest/wiki/Problems#already-exists"
          }, duplicate))
          "Second create result unexpected"

    ]
    |> Seq.ofList