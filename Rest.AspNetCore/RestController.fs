namespace Rest.AspNetCore

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Mvc.ModelBinding
open Rest

[<ApiController>]
type RestController<'Key,'Entity>(resource : RestResource<'Key,'Entity>) =
  inherit Controller()

  let invoke = resource.Handler >> RestActionResult.fromResult

  [<HttpDelete("{key}")>]
  member this.Delete ([<FromRoute>] key : 'Key) =
    Delete key |> invoke

  [<HttpGet("{key}")>]
  member this.Get ([<FromRoute>] key : 'Key) =
    Get key |> invoke

  [<HttpGet>]
  member this.List () =
    List () |> invoke

  [<HttpPost>]
  member this.Post ([<FromBody>] entity : 'Entity) =
    Post entity |> invoke

  [<HttpPut("{key}")>]
  member this.Put ([<FromRoute>] key : 'Key) ([<FromBody>] entity : 'Entity) =
    Put (key, entity) |> invoke