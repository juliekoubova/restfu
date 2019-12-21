namespace Rest
open RestFail

type RestHandler<'Key, 'Entity> =
  RestRequest<'Key, 'Entity> -> RestResult<'Key, 'Entity>

module RestHandler =

  let empty : RestHandler<'K, 'E> =
    function
    | Delete key -> DeleteFail (notFound, key)
    | Get key -> GetFail (notFound, key)
    | Query query -> QueryFail (notFound, query)
    | Post entity -> PostFail (notFound, entity)
    | Put (key, entity) -> PutFail (notFound, (key, entity))

  let withDelete delete handler : RestHandler<'K, 'E> =
    function
    | Delete key -> (delete handler key)
    | req -> (handler req)

  let withGet get handler : RestHandler<'K, 'E>  =
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
    handler |> withDelete (fun _ key -> DeleteFail (methodNotAllowed, key))

  let withoutGet handler : RestHandler<'K, 'E> =
    handler |> withGet (fun _ key -> GetFail (methodNotAllowed, key))

  let withoutPost handler : RestHandler<'K, 'E> =
    handler |> withPost (fun _ entity -> PostFail (methodNotAllowed, entity))

  let withoutPut handler : RestHandler<'K, 'E> =
    handler |> withPut (fun _ (key, entity) -> PutFail (methodNotAllowed, (key, entity)))

  let withoutQuery handler : RestHandler<'K, 'E> =
    handler |> withQuery (fun _ query -> QueryFail (methodNotAllowed, query))