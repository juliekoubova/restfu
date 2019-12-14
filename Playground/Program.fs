open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Rest
open System

let validatePutKey entityKey resource: RestResource<'Key, 'Entity> =
  function
  | Put (key, entity) as req ->
      let keyFromEntity = entityKey entity
      if key = keyFromEntity then
        resource req
      else
        PutFail ((RestFail.cannotChangeKey key keyFromEntity), key, entity)
  | req -> resource req

type Pet = {
  Name : String
  Owner : String
}

let petKey pet = pet.Name
let pets = InMemory.create petKey |> validatePutKey petKey

let configureApp app =
  ()

[<EntryPoint>]
let main _ =
  WebHost.CreateDefaultBuilder()
    .Configure(configureApp)
    .Build()
    .Run()
  0
  // printfn "POST: %A" (pets <| Post { Name = "Moan"; Owner = "Daddy" })
  // printfn "LIST: %A" (pets <| List)
  // printfn "PUT: %A" (pets <| Put ("Moan", { Name = "Penny"; Owner = "Daddy" }))