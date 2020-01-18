[<AutoOpen>]
module Rest.AspNetCore.Extensions
open Rest

open Microsoft.AspNetCore.Mvc.ApplicationModels
open Microsoft.Extensions.DependencyInjection

type IServiceCollection with
  member this.AddRest () =
    ignore <| this.AddTransient<IApplicationModelProvider, RestApplicationModelProvider>()
    ignore <| this.ConfigureSwaggerGen(fun swagger ->
      swagger.OperationFilter<SwaggerOperationFilter>()
      swagger.SchemaFilter<SwaggerRestExprModelFilter>()
    )

  member this.AddRestResource<'Key, 'Entity>
    (
      url : string,
      resource : RestResource<'Key, 'Entity>
    ) =
    let res = RestApiExplorer.applyEntityKeyName resource
    this.AddSingleton<IRestApiRegistration>(RestApiRegistration (url, res))