namespace Rest.AspNetCore
open ApplicationModel
open Rest

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Mvc.ApplicationModels
open Microsoft.AspNetCore.Mvc.Routing
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

  let private ofType<'T> =
    Seq.filter (fun a -> box a :? 'T) >> Seq.cast<'T>

  let private singleSelectorModel (attributes : obj seq) =
    attributes
    |> ofType<IRouteTemplateProvider>
    |> Seq.tryHead
    |> Option.map (fun route ->
      let httpMethods =
        match route with
        | :? IActionHttpMethodProvider as httpMethod -> httpMethod.HttpMethods
        | _ -> Seq.empty
        |> Seq.toArray

      selectorModel route httpMethods
    )


  let create (reg : IRestApiRegistration) =
    let k = reg.Resource.KeyType
    let e = reg.Resource.EntityType
    let typeInfo = controllerType k e

    let addSingleSelector (actionModel : ActionModel) =
      singleSelectorModel actionModel.Attributes
      |> Option.iter actionModel.Selectors.Add
      actionModel

    let actions =
      reg.Resource.Methods
      |> Seq.map (
        actionMethodName
        >> typeInfo.GetDeclaredMethod
        >> actionModel
        >> addSingleSelector
      )

    let attribs =
      attributes typeInfo
      |> Collections.List.filter (fun a -> not <| a :? NonControllerAttribute)

    let result = controllerModel typeInfo attribs actions

    singleSelectorModel [RouteAttribute (reg.Url) |> box]
    |> Option.iter result.Selectors.Add

    result

type RestApplicationModelProvider(regs : IRestApiRegistration seq) =
  interface IApplicationModelProvider with
    member this.Order = -10_000
    member this.OnProvidersExecuted _ =
      ()
    member this.OnProvidersExecuting context =
      regs
      |> Seq.map RestControllerModel.create
      |> Seq.iter context.Result.Controllers.Add