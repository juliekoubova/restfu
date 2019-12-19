module internal Rest.AspNetCore.ApplicationModel

open Microsoft.AspNetCore.Mvc.ApplicationModels
open Microsoft.AspNetCore.Mvc.Routing
open Microsoft.AspNetCore.Mvc.ActionConstraints
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
