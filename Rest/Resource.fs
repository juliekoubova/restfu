namespace Rest
open Internal

open System
open System.Reflection

module RestOperations =
  type Op =
  | Delete
  | Get
  | List
  | Post
  | Put

type IRestResource =
  abstract member EntityType : TypeInfo with get
  abstract member KeyType : TypeInfo with get
  abstract member Invoke : Object -> Object
  abstract member Operations : Set<RestOperations.Op> with get

type RestResource<'Key, 'Entity> =
  {
    Handler: RestHandler<'Key, 'Entity>
    Operations: Set<RestOperations.Op>
  }
  interface IRestResource with
    member _.EntityType with get () = typeof<'Entity>.GetTypeInfo()
    member _.KeyType with get () = typeof<'Key>.GetTypeInfo()
    member this.Operations with get () = this.Operations
    member this.Invoke request =
      upcast this.Handler (request :?> RestRequest<'Key, 'Entity>)

module RestResource =

  let empty = {
    Handler = RestHandler.empty
    Operations = Set.empty
  }

  let map h m resource = {
    Handler = h resource.Handler
    Operations = m resource.Operations
  }

  let mapHandler h =
    map h id

  let private withBuilder
    (handler: 'F -> RestHandler<'K,'E> -> RestHandler<'K, 'E>)
    (oepration: RestOperations.Op)
    f
    =
    map (handler f) (Set.add oepration)

  let private withoutBuilder
    (handler: RestHandler<'K,'E> -> RestHandler<'K, 'E>)
    (operation: RestOperations.Op)
    =
    map handler (Set.remove operation)

  let withDelete f =
    withBuilder RestHandler.withDelete RestOperations.Delete f

  let withGet f =
    withBuilder RestHandler.withGet RestOperations.Get f

  let withList f =
    withBuilder RestHandler.withList RestOperations.List f

  let withPost f =
    withBuilder RestHandler.withPost RestOperations.Post f

  let withPut f =
    withBuilder RestHandler.withPut RestOperations.Put f

  let withoutDelete handler =
    handler |> withoutBuilder (RestHandler.withoutDelete) RestOperations.Delete

  let withoutGet handler =
    handler |> withoutBuilder RestHandler.withoutGet RestOperations.Get

  let withoutList handler =
    handler |> withoutBuilder RestHandler.withoutList RestOperations.List

  let withoutPost handler =
    handler |> withoutBuilder RestHandler.withoutPost RestOperations.Post

  let withoutPut handler =
    handler |> withoutBuilder RestHandler.withoutPut RestOperations.Put

  let create delete get list post put =
    empty
    |> withDelete (ignoreArg0 delete)
    |> withGet (ignoreArg0 get)
    |> withList (ignoreArg0 list)
    |> withPost (ignoreArg0 post)
    |> withPut (ignoreArg0 put)