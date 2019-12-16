[<AutoOpen>]
module Rest.AspNetCore.Extensions
open Rest

open Microsoft.AspNetCore.Mvc.ApplicationModels
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.DependencyInjection.Extensions

type IServiceCollection with
  member this.AddRest () =
    this.AddSingleton<
      IApplicationModelProvider,
      RestApplicationModelProvider
      > ()

  member this.AddRestResource (url: string) (resource : IRestResource) =
    this.AddSingleton<IRestApiRegistration> (RestApiRegistration(url, resource))