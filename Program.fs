open System
open Rest

let validatePutId idSelector resource: RestResource<'Id, 'Entity> =
  function
  | Put (id, entity) as req ->
      if id = idSelector entity then
        (resource req)
      else
        PutFail (BadRequest, id, entity)
  | req -> resource req

let convertId resource convert convertBack: RestResource<'Id, 'Entity> =
 fun req ->
    req
    |> RestRequest.mapId convert
    |> resource
    |> RestResult.mapId convertBack

type Pet = {
  Id : String
  Owner : String
}

let petId pet = pet.Id
let pets = InMemory.create petId |> validatePutId petId

[<EntryPoint>]
let main _ =
  printfn "POST: %A" (pets <| Post { Id = "Moan"; Owner = "Daddy" })
  printfn "LIST: %A" (pets <| List)
  printfn "PUT: %A" (pets <| Put ("Moan", { Id = "Penny"; Owner = "Daddy" }))
  0