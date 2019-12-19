namespace Rest.AspNetCore

open Microsoft.AspNetCore.Mvc
open Rest

[<ApiController>]
[<NonController>]
type RestController<'Key, 'Entity>(resource : RestResource<'Key, 'Entity>) =
  inherit Controller()

  let invoke = resource.Handler >> RestActionResult.fromResult

  [<HttpDelete("{key}")>]
  member _.Delete ([<FromRoute>] key : 'Key) =
    Delete key |> invoke

  [<HttpGet("{key}")>]
  member _.Get ([<FromRoute>] key : 'Key) =
    Get key |> invoke

  [<HttpGet>]
  member _.List () =
    List () |> invoke

  [<HttpPost>]
  member _.Post ([<FromBody>] entity : 'Entity) =
    Post entity |> invoke

  [<HttpPut("{key}")>]
  member _.Put ([<FromRoute>] key : 'Key, [<FromBody>] entity : 'Entity) =
    Put (key, entity) |> invoke

