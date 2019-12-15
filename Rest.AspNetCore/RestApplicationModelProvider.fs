namespace Rest.AspNetCore
open Microsoft.AspNetCore.Mvc.ApplicationModels

type RestApplicationModelProvider() =
  interface IApplicationModelProvider with
    member this.Order = 0
    member this.OnProvidersExecuted _ =
      ()
    member this.OnProvidersExecuting context =
      context.Result.Controllers.Add(

      )
      ()