[<AutoOpen>]
module Rest.AspNetCore.Extensions

open Microsoft.AspNetCore.Mvc.ApplicationModels
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.DependencyInjection.Extensions

type IServiceCollection with
  member this.AddRest () =
    this.TryAddSingleton<
      IApplicationModelProvider,
      RestApplicationModelProvider
      > ()
    this