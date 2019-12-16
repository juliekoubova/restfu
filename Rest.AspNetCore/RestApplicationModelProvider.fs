namespace Rest.AspNetCore
open Rest

open Microsoft.AspNetCore.Mvc.ApplicationModels
open System.Reflection

module RestControllerModel =
  let private controllerType k e =
    let typedef = typedefof<RestController<_,_>>
    let generic = typedef.MakeGenericType (k, e)
    generic.GetTypeInfo()

  let private actionMethodName =
    function
    | RestMethods.Delete -> "Delete"
    | RestMethods.Get -> "Get"
    | RestMethods.List -> "List"
    | RestMethods.Post -> "Post"
    | RestMethods.Put -> "Put"

  let addAction (controller : ControllerModel) (action : ActionModel) =
    controller.Actions.Add action
    action.Controller <- controller

  let controllerModel typeInfo attributes actions =
    let controller = ControllerModel (typeInfo, attributes)
    actions |> Seq.iter (addAction controller)
    controller

  let create (reg : IRestApiRegistration) =
    let k = reg.Resource.KeyType
    let e = reg.Resource.EntityType
    let typeInfo = controllerType k e
    let actionModel =
      actionMethodName
      >> typeInfo.GetDeclaredMethod
      >> (fun mi -> ActionModel(mi, []))

    let actions =
      reg.Resource.Methods
      |> Seq.map actionModel

    controllerModel typeInfo [] actions

type RestApplicationModelProvider(regs : IRestApiRegistration seq) =
  interface IApplicationModelProvider with
    member this.Order = -10_000
    member this.OnProvidersExecuted _ =
      ()
    member this.OnProvidersExecuting context =
      regs
      |> Seq.map RestControllerModel.create
      |> Seq.iter context.Result.Controllers.Add
      ()