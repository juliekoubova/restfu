namespace Restfu.AspNetCore
open Microsoft.AspNetCore.Mvc.ApplicationModels
open Microsoft.AspNetCore.Mvc.ModelBinding

type internal RestApplicationModelProvider
  (
    registrations : IRestApiRegistration seq,
    modelMetadataProvider : IModelMetadataProvider
  )
  =

  let create =
    match modelMetadataProvider with
    | :? ModelMetadataProvider as mmp -> Some mmp
    | _ -> None
    |> RestControllerModel.create

  interface IApplicationModelProvider with
    member _.Order = -10_000
    member _.OnProvidersExecuted _ =
      ()
    member _.OnProvidersExecuting context =
      registrations
      |> Seq.map create
      |> Seq.iter context.Result.Controllers.Add