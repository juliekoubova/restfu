module Rest.AspNetCore.RestControllerModel
open Rest
open ApplicationModel
open Reflection
open RestResourceProperty

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Mvc.ApplicationModels
open Microsoft.AspNetCore.Mvc.ModelBinding
open Microsoft.AspNetCore.Mvc.Routing
open System.Reflection

let private controllerType k e =
  let typedef = typedefof<RestController<_,_>>
  let generic = typedef.MakeGenericType (k, e)
  generic.GetTypeInfo()

let private actionMethodName =
  function
  | RestOperations.Delete -> "Delete"
  | RestOperations.Get -> "Get"
  | RestOperations.Post -> "Post"
  | RestOperations.Put -> "Put"
  | RestOperations.Query -> "Query"

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

let create
  (mmp : ModelMetadataProvider option)
  (reg : IRestApiRegistration)
  =
  let k = reg.Resource.KeyType
  let e = reg.Resource.EntityType
  let typeInfo = controllerType k e

  let createParameter (parameter : ParameterInfo) =
    let attribs = attributes parameter
    let bindingInfo =
      match mmp with
      | None -> BindingInfo.GetBindingInfo attribs
      | Some mmp ->
        let metadata = mmp.GetMetadataForParameter parameter
        BindingInfo.GetBindingInfo (attribs, metadata)

    ParameterModel (
      parameter,
      attribs,
      BindingInfo = bindingInfo,
      ParameterName = parameter.Name
    )

  let createAction (operationType : RestOperations.RestOperationType) =
    let methodInfo =
      operationType
      |> actionMethodName
      |> typeInfo.GetDeclaredMethod

    let attribs =
      attributes methodInfo

    let parameters =
      methodInfo.GetParameters ()
      |> Seq.map createParameter

    let selector =
      singleSelectorModel attribs
      |> Option.toList

    actionModel methodInfo attribs parameters selector

  let actions =
    reg.Resource.Operations
    |> Seq.map (fun kvp -> createAction kvp.Key)

  let attribs =
    attributes typeInfo
    |> List.filter (fun a -> not <| a :? NonControllerAttribute)

  let result = controllerModel typeInfo attribs actions

  singleSelectorModel [RouteAttribute (reg.Url) |> box]
  |> Option.iter result.Selectors.Add

  setResource result.Properties reg.Resource
  result
