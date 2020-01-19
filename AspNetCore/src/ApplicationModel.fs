module internal Rest.AspNetCore.ApplicationModel

open Microsoft.AspNetCore.Mvc.ActionConstraints
open Microsoft.AspNetCore.Mvc.ApplicationModels
open Microsoft.AspNetCore.Mvc.Routing
open Microsoft.AspNetCore.Routing
open System.Reflection
open Microsoft.AspNetCore.Mvc

let actionModel
 (methodInfo : MethodInfo)
 (attributes : obj list)
 (parameters : ParameterModel seq)
 (selectors  : SelectorModel seq)
 =
  let model = ActionModel (methodInfo, attributes)
  parameters |> Seq.iter (fun p -> p.Action <- model)
  parameters |> Seq.iter model.Parameters.Add
  selectors |> Seq.iter model.Selectors.Add
  model

let controllerModel typeInfo attributes actions =
  let controller = ControllerModel (typeInfo, attributes)

  let addAction action =
    controller.Actions.Add action
    action.Controller <- controller

  actions |> Seq.iter addAction
  controller

let selectorModel
  (route : IRouteTemplateProvider)
  (httpMethods : string array)
  =
  let attributeRouteModel =
    AttributeRouteModel (route)

  let selectorModel =
    SelectorModel (AttributeRouteModel = attributeRouteModel)

  if (httpMethods |> Array.isEmpty |> not) then
    selectorModel.ActionConstraints.Add (HttpMethodActionConstraint httpMethods)
    selectorModel.EndpointMetadata.Add (HttpMethodMetadata httpMethods)

  selectorModel

let modifyRouteTemplate (modify : string -> string) (attr : obj) =

  let mapNull f o =
    match o with
    | null -> None
    | x -> Some (f x)

  let modifyTemplate = mapNull modify

  let copyHMA (source : HttpMethodAttribute) (target : HttpMethodAttribute) =
    target.Name <- source.Name
    target.Order <- source.Order
    target

  let modifyHMA (hma : HttpMethodAttribute) =
    let t = hma.GetType ()
    let args =
      modifyTemplate hma.Template
      |> Option.map box
      |> Option.toArray

    System.Activator.CreateInstance  (t, args)
    :?> HttpMethodAttribute
    |> copyHMA hma
    |> box

  match attr with
  | :? HttpMethodAttribute as hma -> modifyHMA hma
  | :? IRouteTemplateProvider ->
    failwithf "Unable to modify route template for %A" attr
  | other -> box other