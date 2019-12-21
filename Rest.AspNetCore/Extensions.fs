[<AutoOpen>]
module Rest.AspNetCore.Extensions
open Rest

open Microsoft.AspNetCore.Mvc.ApplicationModels
open Microsoft.Extensions.DependencyInjection

type IServiceCollection with
  member this.AddRest () =
    ignore <| this.AddTransient<IApplicationModelProvider, RestApplicationModelProvider> ()
    ignore <| this.ConfigureSwaggerGen(fun swagger ->
      swagger.OperationFilter<SwaggerOperationFilter> ()
    )

  member this.AddRestResource (url: string) (resource : IRestResource) =
    this.AddSingleton<IRestApiRegistration> (RestApiRegistration (url, resource))