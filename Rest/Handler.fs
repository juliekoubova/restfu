namespace Rest
open RestFailDefinition

type RestHandler<'Key, 'Entity> =
  RestRequest<'Key, 'Entity> -> RestResult<'Key, 'Entity>

type RestHandlerTransform<'K, 'E, 'Req> =
  RestHandler<'K, 'E> -> 'Req -> RestResult<'K, 'E>

module RestHandler =

  let private applyMethodNotAllowed x =
    applyFail methodNotAllowed () x

  let private applyNotFound x =
    applyFail notFound () x

  let empty : RestHandler<'K, 'E> =
    function
    | Delete key -> DeleteFail (applyNotFound key)
    | Get key -> GetFail (applyNotFound key)
    | Post entity -> PostFail (applyNotFound entity)
    | Put (key, entity) -> PutFail (applyNotFound (key, entity))
    | Query query -> QueryFail (applyNotFound query)

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
    | Get key -> (get handler key)
    | req -> (handler req)

  let withPost post handler : RestHandler<'K, 'E>  =
    function
    | Post entity -> (post handler entity)
    | req -> (handler req)

  let withPut put handler : RestHandler<'K, 'E>  =
    function
    | Put (key, entity) -> (put handler (key, entity))
    | req -> (handler req)

  let withQuery query handler : RestHandler<'K, 'E>  =
    function
    | Query q -> (query handler q)
    | req -> (handler req)

  let withoutDelete handler : RestHandler<'K, 'E> =
    handler |> withDelete (fun _ x -> DeleteFail (applyMethodNotAllowed x))

  let withoutGet handler : RestHandler<'K, 'E> =
    handler |> withGet (fun _ x -> GetFail (applyMethodNotAllowed x))

  let withoutPost handler : RestHandler<'K, 'E> =
    handler |> withPost (fun _ x -> PostFail (applyMethodNotAllowed x))

  let withoutPut handler : RestHandler<'K, 'E> =
    handler |> withPut (fun _ x -> PutFail (applyMethodNotAllowed x))

  let withoutQuery handler : RestHandler<'K, 'E> =
    handler |> withQuery (fun _ x -> QueryFail (applyMethodNotAllowed x))