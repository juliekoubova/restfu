namespace Rest
open Internal

open System
open System.Reflection

module RestMethods =
  type RestMethod =
  | Delete
  | Get
  | List
  | Post
  | Put

type IRestResource =
  abstract member EntityType : TypeInfo with get
  abstract member KeyType : TypeInfo with get
  abstract member Invoke : Object -> Object
  abstract member Methods : Set<RestMethods.RestMethod> with get

type RestResource<'Key, 'Entity> =
  {
    Handler: RestHandler<'Key, 'Entity>
    Methods: Set<RestMethods.RestMethod>
  }
  interface IRestResource with
    member _.EntityType with get () = typeof<'Entity>.GetTypeInfo()
    member _.KeyType with get () = typeof<'Key>.GetTypeInfo()
    member this.Methods with get () = this.Methods
    member this.Invoke request =
      upcast this.Handler (request :?> RestRequest<'Key, 'Entity>)

module RestResource =

  let empty = {
    Handler = RestHandler.empty
    Methods = Set.empty
  }

  let map h m resource = {
    Handler = h resource.Handler
    Methods = m resource.Methods
  }

  let mapHandler h =
    map h id

  let private withBuilder
    (handlerBuilder: 'F -> RestHandler<'K,'E> -> RestHandler<'K, 'E>)
    (handlerMethod: RestMethods.RestMethod)
    f
    =
    map (handlerBuilder f) (Set.add handlerMethod)

  let private withoutBuilder
    (handlerBuilder: RestHandler<'K,'E> -> RestHandler<'K, 'E>)
    (handlerMethod: RestMethods.RestMethod)
    =
    map handlerBuilder (Set.remove handlerMethod)

  let withDelete f =
    withBuilder RestHandler.withDelete RestMethods.Delete f

  let withGet f =
    withBuilder RestHandler.withGet RestMethods.Get f

  let withList f =
    withBuilder RestHandler.withList RestMethods.List f

  let withPost f =
    withBuilder RestHandler.withPost RestMethods.Post f

  let withPut f =
    withBuilder RestHandler.withPut RestMethods.Put f

  let withoutDelete handler =
    handler |> withoutBuilder (RestHandler.withoutDelete) RestMethods.Delete

  let withoutGet handler =
    handler |> withoutBuilder RestHandler.withoutGet RestMethods.Get

  let withoutList handler =
    handler |> withoutBuilder RestHandler.withoutList RestMethods.List

  let withoutPost handler =
    handler |> withoutBuilder RestHandler.withoutPost RestMethods.Post

  let withoutPut handler =
    handler |> withoutBuilder RestHandler.withoutPut RestMethods.Put

  let create delete get list post put =
    empty
    |> withDelete (ignoreArg0 delete)
    |> withGet (ignoreArg0 get)
    |> withList (ignoreArg0 list)
    |> withPost (ignoreArg0 post)
    |> withPut (ignoreArg0 put)