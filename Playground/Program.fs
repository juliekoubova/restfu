open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.OpenApi.Models
open System
open Rest
open Rest.AspNetCore
open RestFail

let validatePutKey entityKey =
  RestResource.withPut (
    fun handler (key, entity) ->
      let keyFromEntity = entityKey entity
      if key = keyFromEntity then
        Put (key, entity) |> handler
      else
        PutFail ((cannotChangeKey key keyFromEntity), (key, entity))
  )

[<CLIMutable>]
type Pet = {
  Name : String
  Owner : String
}

let petKey pet = pet.Name
let pets = InMemory.create petKey |> validatePutKey petKey

let configureServices (services : IServiceCollection) =
  services.AddMvc () |> ignore
  services.AddRest () |> ignore
  services.AddRestResource "pets" pets |> ignore
  services.AddSwaggerGen (fun swagger ->
    swagger.SwaggerDoc ("v1", OpenApiInfo(Title = "Pets API", Version = "v1"))
  ) |> ignore

let configureApp (app : IApplicationBuilder) =
  app.UseSwagger () |> ignore
  app.UseSwaggerUI (fun c ->
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pets API")
  ) |> ignore
  ()

[<EntryPoint>]
let main _ =
  WebHost.CreateDefaultBuilder()
    .ConfigureLogging(fun log ->
      log.AddConsole () |> ignore
      log.AddFilter ("Microsoft", LogLevel.Debug) |> ignore
      ()
    )
    .ConfigureServices(configureServices)
    .Configure(configureApp)
    .Build()
    .Run()
  // let h = pets.Handler
  // printfn "POST: %A" (h <| Post { Name = "Moan"; Owner = "Daddy" })
  // printfn "LIST: %A" (h <| List ())
  // printfn "PUT: %A" (h <| Put ("Moan", { Name = "Penny"; Owner = "Daddy" }))
  0