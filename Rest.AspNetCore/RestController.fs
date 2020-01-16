namespace Rest.AspNetCore
open Rest
open Rest.AspNetCore
open Rest.Internal
open RestResourceProperties

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Mvc.ModelBinding
open System

type RestExprModelBinder () =
  interface IModelBinder with
    member _.BindModelAsync context =
      let context =
        if isNull context
        then invalidArg "context" "Context cannot be null"
        else context

      let value = context.ValueProvider.GetValue context.ModelName
      if value = ValueProviderResult.None
        System.Threading.Tasks.Task.CompletedTask
      else
        RestExpr.parse value.FirstValue

[<ApiController>]
[<NonController>]
type RestController<'Key, 'Entity> () =
  inherit ControllerBase()

  let parseFilter str =
    match str with
    | null -> Result.Ok None
    | str -> RestExpr.parse<'Entity> str |> Result.map Some

  member private this.Invoke (request : RestRequest<'Key, 'Entity>) =
    this.ControllerContext.ActionDescriptor.Properties
    |> getResource<'Key, 'Entity>
    |> (fun res -> res.Handler request)
    |> RestActionResult.fromResult<'Key, 'Entity>

  // NOTE: Routes must be specified using the RouteAttribute separately from
  // method constraints, because the Route attribute gets replaced when
  // RestControllerModel renames the key parameter.

  [<HttpDelete("{key}")>]
  member this.Delete
    (
      [<FromRoute(Name = "key")>] key : 'Key
    ) =
    Delete key |> this.Invoke

  [<HttpGet("{key}")>]
  member this.Get
    (
      [<FromRoute(Name = "key")>] key : 'Key
    ) =
    Get key |> this.Invoke

  [<HttpPatch("{key}")>]
  member this.Patch
    (
      [<FromRoute(Name = "key")>] key : 'Key,
      [<FromBody>]                patch : JsonPatch
    ) =
    Patch (key, patch) |> this.Invoke

  [<HttpPost>]
  member this.Post
    (
      [<FromBody>] entity : 'Entity
    ) =
    Post entity |> this.Invoke

  [<HttpPut("{key}")>]
  member this.Put
    (
      [<FromRoute(Name = "key")>] key : 'Key,
      [<FromBody>]                entity : 'Entity
    ) =
    Put (key, entity) |> this.Invoke

  [<HttpGet>]
  member this.Query
    (
      [<FromQuery(Name = "$filter")>]
      filter : string,

      // [<FromQuery(Name = "$orderby")>]
      // orderBy: string [],

      [<FromQuery(Name = "$skip")>]
      skip : Nullable<int32>,

      [<FromQuery(Name = "$skiptoken")>]
      skipToken : string,

      [<FromQuery(Name = "$top")>]
      top: Nullable<int32>
    )
    =
    let query =
      parseFilter filter
      |> Result.map (fun filter -> {
        Filter = filter
        OrderBy = []
        Skip = nullableToOption skip
        SkipToken = nonEmptyStringToOption skipToken
        Top = nullableToOption top
      })

    match query with
    | Error err -> this.BadRequest

    Query query |> this.Invoke