open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.OpenApi.Models
open MongoDB.Bson.Serialization.Attributes
open MongoDB.Driver
open Restfu
open Restfu.AspNetCore
open Restfu.MongoDB
open System
open System.ComponentModel.DataAnnotations

[<CLIMutable>]
type Pet = {
  [<Required>]
  Name : string

  [<Required>]
  Owner : string

  [<Required; Range(18, 100)>]
  Age : int

  [<Required>]
  Thicc : bool
}

[<CLIMutable>]
type Toy = {
  [<BsonId; Required>]
  Sku : string

  [<Required>]
  Length : float

  [<Required>]
  Diameter : float
}

let mongoClient =
  Environment.GetEnvironmentVariable("MONGODB_URL")
  |> NonEmptyString.create
  |> Option.orElse (NonEmptyString.create "mongodb://localhost:27017")
  |> (Option.get >> fun nes -> nes.Value)
  |> MongoClient

let playgroundDatabase =
  mongoClient.GetDatabase("playground")

let toyCollection = playgroundDatabase.GetCollection<Toy>("toys")
let toys = MongoDBResource.Create(toyCollection, fun toy -> toy.Sku)

let pets = InMemory.Create (fun pet -> pet.Name)
let pet = Post >> pets.Handler >> ignore
pet { Name = "Moan"; Owner = "Daddy"; Age = 33; Thicc = true }

let configureServices (services : IServiceCollection) =
  ignore <| services.AddControllers()
  ignore <| services.AddRest()
  ignore <| services.AddRestResource("pets", pets)
  ignore <| services.AddRestResource("toys", toys)
  ignore <| services.AddSwaggerGen(fun swagger ->
    swagger.SwaggerDoc("pets", OpenApiInfo (Title = "Pets API", Version = "v1"))
  )

let configureApp (app : IApplicationBuilder) =
  ignore <| app.UseRouting()
  ignore <| app.UseEndpoints(fun endpoints ->
    ignore <| endpoints.MapControllers()
  )
  ignore <| app.UseSwagger()
  ignore <| app.UseSwaggerUI(fun c ->
    ignore <| c.SwaggerEndpoint("/swagger/pets/swagger.json", "Pets API")
  )

[<EntryPoint>]
let main _ =
  WebHost.CreateDefaultBuilder()
    .ConfigureServices(configureServices)
    .Configure(configureApp)
    .Build()
    .Run()
  0