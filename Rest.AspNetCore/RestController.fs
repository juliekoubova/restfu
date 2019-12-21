namespace Rest.AspNetCore
open Rest
open RestResourceProperties

open Microsoft.AspNetCore.Mvc

[<ApiController>]
[<NonController>]
type RestController<'Key, 'Entity>() =
  inherit ControllerBase()

  member private this.Invoke (request : RestRequest<'Key, 'Entity>) =
    let props = this.ControllerContext.ActionDescriptor.Properties
    match getResource<'Key, 'Entity> props with
    | Some resource ->
      resource.Handler request |> RestActionResult.fromResult<'Key, 'Entity>
    | None -> upcast this.StatusCode(500)

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