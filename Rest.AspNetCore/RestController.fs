namespace Rest.AspNetCore
open Rest
open Rest.AspNetCore
open RestResourceProperties

open Microsoft.AspNetCore.Mvc
open System

[<ApiController>]
[<NonController>]
type RestController<'Key, 'Entity>() =
  inherit ControllerBase()

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

      [<FromQuery(Name = "$orderby")>]
      orderBy: string [],

      [<FromQuery(Name = "$skip")>]
      skip : Nullable<int32>,

      [<FromQuery(Name = "$skiptoken")>]
      skipToken : string,

      [<FromQuery(Name = "$top")>]
      top: Nullable<int32>
    )
    =
    Query None |> this.Invoke