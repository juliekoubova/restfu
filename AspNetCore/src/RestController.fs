namespace Restfu.AspNetCore
open Restfu
open Restfu.AspNetCore
open RestResourceProperties

open Microsoft.AspNetCore.Mvc
open System
open System.Threading

[<ApiController>]
[<NonController>]
type RestController<'Key, 'Entity>() =
  inherit ControllerBase()

  member private this.Invoke (request : RestRequest<'Key, 'Entity>) =
    let resource =
      getResource<'Key, 'Entity>
        this.ControllerContext.ActionDescriptor.Properties

    let workflow = async {
      let! result = resource.Handler request
      return RestActionResult.fromResult result
    }

    Async.StartImmediateAsTask (workflow, this.HttpContext.RequestAborted)

  [<HttpDelete("{key}")>]
  member this.Delete
    (
      [<FromRoute(Name = "key")>] key : 'Key
    )
    =
    Delete key |> this.Invoke

  [<HttpGet("{key}")>]
  member this.Get
    (
      [<FromRoute(Name = "key")>] key : 'Key
    )
    =
    Get key |> this.Invoke

  [<HttpPatch("{key}")>]
  member this.Patch
    (
      [<FromRoute(Name = "key")>]
      key : 'Key,

      [<FromBody>]
      patch : JsonPatch
    )
    =
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
      [<FromRoute(Name = "key")>]
      key : 'Key,

      [<FromBody>]
      entity : 'Entity
    )
    =
    Put (key, entity) |> this.Invoke

  [<HttpGet>]
  member this.Query
    (
      [<FromQuery(Name = "$filter")>]
      filter : RestExprModel<'Entity>,

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
    let filter =
      match filter with
      | null -> None
      | f -> Some f.Expression

    let query = {
      Filter = filter
      OrderBy = []
      Skip = nullableToOption skip
      // SkipToken = nonEmptyStringToOption skipToken
      Top = nullableToOption top
    }
    Query query |> this.Invoke