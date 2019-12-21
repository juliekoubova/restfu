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

[<Route("handmade")>]
type public HandmadeController() =
  inherit Controller()

  [<HttpGet>]
  [<Produces(typeof<Pet seq>)>]
  member _.List () : IActionResult =
    upcast OkObjectResult Seq.empty<Pet>

let petKey pet = pet.Name
let pets = InMemory.create petKey |> validatePutKey petKey

let configureServices (services : IServiceCollection) =
  services.AddControllers() |> ignore
  services.AddRest () |> ignore
  services.AddRestResource "pets" pets |> ignore
  services.AddSwaggerGen (fun swagger ->
    swagger.SwaggerDoc ("v1", OpenApiInfo(Title = "Pets API", Version = "v1"))
  ) |> ignore

let configureApp (app : IApplicationBuilder) =
  ignore <| app.UseRouting ()
  ignore <| app.UseEndpoints(fun endpoints ->
    endpoints.MapControllers() |> ignore
  )
  ignore <| app.UseSwagger ()
  ignore <| app.UseSwaggerUI (fun c ->
    ignore <| c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pets API")
  )

[<EntryPoint>]
let main _ =
  WebHost.CreateDefaultBuilder()
    .ConfigureLogging(fun log ->
      log.AddConsole () |> ignore
      ()
    )
    .ConfigureServices(configureServices)
    .Configure(configureApp)
    .Build()
    .Run()
  0