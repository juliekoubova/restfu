namespace Rest.AspNetCore
open Rest

open Microsoft.AspNetCore.Mvc.ApplicationModels
open System.Reflection

module RestControllerModel =
  let private controllerType k e =
    let typedef = typedefof<RestController<_,_>>
    let generic = typedef.MakeGenericType (k, e)
    generic.GetTypeInfo()

  let private controllerAction
    (controllerType : TypeInfo)
    (method : RestMethods.RestMethod)
    =
    match method with
    | RestMethods.Delete -> "Delete"
    | RestMethods.Get -> "Get"
    | RestMethods.List -> "List"
    | RestMethods.Post -> "Post"
    | RestMethods.Put -> "Put"
    |> controllerType.GetDeclaredMethod

  let create (reg: IRestApiRegistration) =
    let k = reg.Resource.KeyType
    let e = reg.Resource.EntityType
    let typeInfo = controllerType k e
    let controller = ControllerModel (typeInfo, [])
    let makeAction =
      (controllerAction typeInfo)
      >> (fun mi -> ActionModel(mi, []))

    reg.Resource.Methods
    |> Seq.map makeAction
    |> Seq.iter controller.Actions.Add

    controller

type RestApplicationModelProvider(regs : IRestApiRegistration seq) =
  interface IApplicationModelProvider with
    member this.Order = -10_000
    member this.OnProvidersExecuted _ =
      ()
    member this.OnProvidersExecuting context =
      let zz = regs |> Seq.map RestControllerModel.create


      printf "%A" zz

      zz |> Seq.iter context.Result.Controllers.Add
      ()