module Rest.AspNetCore.RestControllerModel
open Rest
open Rest.Reflection

open ApplicationModel
open RestResourceProperties

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
  | RestOperations.Patch -> "Patch"
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


let private renameKeyFromRoute (keyName : string) (attributes : obj list) =
  attributes
  |> ofType<FromRouteAttribute>
  |> Seq.filter (fun a -> a.Name = "key")
  |> Seq.iter (fun a -> a.Name <- keyName)
  attributes

let private renameRoute (keyName : string) (attributes : obj list) =
  attributes
  |> List.map (modifyRouteTemplate (fun template ->
    template.Replace ("{key}", sprintf "{%s}" keyName)
  ))

let create
  (mmp : ModelMetadataProvider option)
  (reg : IRestApiRegistration)
  =
  let keyName = reg.Resource.KeyName.ToLowerInvariant()
  let keyType = reg.Resource.KeyType
  let entityType = reg.Resource.EntityType
  let typeInfo = controllerType keyType entityType

  let createParameter (parameter : ParameterInfo) =
    let attribs = attributes parameter |> renameKeyFromRoute keyName
    let bindingInfo =
      match mmp with
      | None -> BindingInfo.GetBindingInfo attribs
      | Some mmp ->
        let metadata = mmp.GetMetadataForParameter parameter
        BindingInfo.GetBindingInfo (attribs, metadata)

    // let parameterName =
    //   match parameter.Name with
    //   | "key" -> keyName
    //   | name -> name

    ParameterModel (
      parameter,
      attribs,
      BindingInfo = bindingInfo,
      ParameterName = parameter.Name
    )

  let createAction operationType operation =
    let methodInfo =
      operationType
      |> actionMethodName
      |> typeInfo.GetDeclaredMethod

    let attribs =
      attributes methodInfo |> renameRoute keyName

    let parameters =
      methodInfo.GetParameters () |> Seq.map createParameter

    let selector =
      singleSelectorModel attribs
      |> Option.toList

    actionModel methodInfo attribs parameters selector
    |> setOperation operation

  let actions =
    reg.Resource.Operations
    |> Seq.map (fun kvp -> createAction kvp.Key kvp.Value)

  let attribs =
    attributes typeInfo
    |> List.filter (fun a -> not <| a :? NonControllerAttribute)

  let result = controllerModel typeInfo attribs actions

  singleSelectorModel [RouteAttribute (reg.Url) |> box]
  |> Option.iter result.Selectors.Add

  setResource reg.Resource result
