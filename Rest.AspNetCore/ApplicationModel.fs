module internal Rest.AspNetCore.ApplicationModel

open Microsoft.AspNetCore.Mvc.ApplicationModels
open Microsoft.AspNetCore.Mvc.Routing
open Microsoft.AspNetCore.Mvc.ActionConstraints
open Microsoft.AspNetCore.Routing
open System.Reflection

let attributes (attributeProvider : ICustomAttributeProvider) =
  attributeProvider.GetCustomAttributes false
  |> Seq.cast<obj>
  |> Seq.toList

let actionModel (methodInfo : MethodInfo) =
  ActionModel (methodInfo, (attributes methodInfo))

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
