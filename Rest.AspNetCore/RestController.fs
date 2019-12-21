namespace Rest.AspNetCore

open Microsoft.AspNetCore.Mvc
open Rest

[<ApiController>]
[<NonController>]
type RestController<'Key, 'Entity>() =
  inherit Controller()

  member private this.Invoke =
    this.ControllerContext.ActionDescriptor.Properties
    |> RestResourceProperty.getResource
    |> fun r -> r.Invoke
    >> RestActionResult.fromResult<'Key, 'Entity>

  [<HttpDelete("{key}")>]
  member this.Delete ([<FromRoute>] key : 'Key) =
    Delete key |> this.Invoke

  [<HttpGet("{key}")>]
  member this.Get ([<FromRoute>] key : 'Key) =
    Get key |> this.Invoke

  [<HttpPost>]
  member this.Post ([<FromBody>] entity : 'Entity) =
    Post entity |> this.Invoke

  [<HttpPut("{key}")>]
  member this.Put ([<FromRoute>] key : 'Key, [<FromBody>] entity : 'Entity) =
    Put (key, entity) |> this.Invoke

  [<HttpGet>]
  member this.Query () =
    Query () |> this.Invoke