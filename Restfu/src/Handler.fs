namespace Restfu
open RestFailDefinition

type RestHandler<'Key, 'Entity> =
  RestRequest<'Key, 'Entity> -> Async<RestResult<'Key, 'Entity>>

type RestHandlerTransform<'K, 'E, 'Req> =
  RestHandler<'K, 'E> -> 'Req -> Async<RestResult<'K, 'E>>

module RestHandler =

  let private applyMethodNotAllowed x =
    applyFail methodNotAllowed () x

  let private applyNotFound x =
    applyFail notFound () x

  let empty : RestHandler<'K, 'E> =
    function
    | Delete key -> DeleteFail (applyNotFound key) |> async.Return
    | Get key -> GetFail (applyNotFound key) |> async.Return
    | Patch (key, patch) -> PatchFail (applyNotFound (key, patch)) |> async.Return
    | Post entity -> PostFail (applyNotFound entity) |> async.Return
    | Put (key, entity) -> PutFail (applyNotFound (key, entity)) |> async.Return
    | Query query -> QueryFail (applyNotFound query) |> async.Return

  let withDelete
    (delete : RestHandlerTransform<'K, 'E, 'K>)
    handler
    : RestHandler<'K, 'E>
    =
    function
    | Delete key -> (delete handler key)
    | req -> (handler req)

  let withGet
    (get : RestHandlerTransform<'K, 'E, 'K>)
    handler
    : RestHandler<'K, 'E>
    =
    function
    | Get k -> get handler k
    | req -> handler req

  let withPatch patch handler : RestHandler<'K, 'E> =
    function
    | Patch (k, p) -> patch handler (k, p)
    | req -> handler req

  let withPost post handler : RestHandler<'K, 'E>  =
    function
    | Post e -> post handler e
    | req -> handler req

  let withPut put handler : RestHandler<'K, 'E>  =
    function
    | Put (k, e) -> put handler (k, e)
    | req -> handler req

  let withQuery query handler : RestHandler<'K, 'E>  =
    function
    | Query q -> query handler q
    | req -> handler req

  let withoutDelete handler : RestHandler<'K, 'E> =
    handler |> withDelete (fun _ x -> DeleteFail (applyMethodNotAllowed x) |> async.Return)

  let withoutGet handler : RestHandler<'K, 'E> =
    handler |> withGet (fun _ x -> GetFail (applyMethodNotAllowed x) |> async.Return)

  let withoutPatch handler : RestHandler<'K, 'E> =
    handler |> withPatch (fun _ x -> PatchFail (applyMethodNotAllowed x) |> async.Return)

  let withoutPost handler : RestHandler<'K, 'E> =
    handler |> withPost (fun _ x -> PostFail (applyMethodNotAllowed x) |> async.Return)

  let withoutPut handler : RestHandler<'K, 'E> =
    handler |> withPut (fun _ x -> PutFail (applyMethodNotAllowed x) |> async.Return)

  let withoutQuery handler : RestHandler<'K, 'E> =
    handler |> withQuery (fun _ x -> QueryFail (applyMethodNotAllowed x) |> async.Return)