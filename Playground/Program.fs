open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Rest
open RestFail
open System

let validatePutKey entityKey =
  RestResource.withPut (
    fun handler (key, entity) ->
      let keyFromEntity = entityKey entity
      if key = keyFromEntity then
        Put (key, entity) |> handler
      else
        PutFail ((cannotChangeKey key keyFromEntity), key, entity)
  )

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
  // WebHost.CreateDefaultBuilder()
  //   .Configure(configureApp)
  //   .Build()
  //   .Run()
  let h = pets.Handler
  printfn "POST: %A" (h <| Post { Name = "Moan"; Owner = "Daddy" })
  printfn "LIST: %A" (h <| List ())
  printfn "PUT: %A" (h <| Put ("Moan", { Name = "Penny"; Owner = "Daddy" }))
  0