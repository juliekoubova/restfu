open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.OpenApi.Models
open System.ComponentModel.DataAnnotations
open Rest
open Rest.AspNetCore

[<CLIMutable>]
type Pet = {
  [<Required>] Name : string
  [<Required>] Owner : string
}

let pets = InMemory.create <@ fun pet -> pet.Name @>
pets.Handler <| Post { Name = "Moan"; Owner = "Daddy" } |> ignore

let configureServices (services : IServiceCollection) =
  ignore <| services.AddControllers()
  ignore <| services.AddRest ()
  ignore <| services.AddRestResource ("pets", pets)
  ignore <| services.AddSwaggerGen (fun swagger ->
    swagger.SwaggerDoc ("pets", OpenApiInfo (Title = "Pets API", Version = "v1"))
  )

let configureApp (app : IApplicationBuilder) =
  ignore <| app.UseRouting ()
  ignore <| app.UseEndpoints (fun endpoints ->
    ignore <| endpoints.MapControllers ()
  )
  ignore <| app.UseSwagger ()
  ignore <| app.UseSwaggerUI (fun c ->
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