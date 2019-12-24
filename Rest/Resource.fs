namespace Rest

open System
open System.Reflection

type IRestResource =
  abstract member EntityName : string with get
  abstract member EntityType : TypeInfo with get
  abstract member KeyName : string with get
  abstract member KeyType : TypeInfo with get
  abstract member Operations : RestOperationMap with get

type RestResource<'Key, 'Entity> =
  {
    EntityName : string
    Handler    : RestHandler<'Key, 'Entity>
    KeyName    : string
    Operations : RestOperationMap
  }
  interface IRestResource with
    member this.EntityName with get () = this.EntityName
    member _.EntityType with get () = typeof<'Entity>.GetTypeInfo()
    member this.KeyName with get () = this.KeyName
    member _.KeyType with get () = typeof<'Key>.GetTypeInfo()
    member this.Operations with get () = this.Operations


module RestResource =

  let empty = {
    EntityName = "Entity"
    Handler = RestHandler.empty
    KeyName = "Key"
    Operations = Map.empty
  }

  let map h o resource = {
    EntityName = resource.EntityName
    Handler = h resource.Handler
    KeyName = resource.KeyName
    Operations = o resource.Operations
  }

  let mapHandler h =
    map h id

  let private withBuilder<'K, 'E, 'Req>
    (opType : RestOperations.RestOperationType)
    (applyTransformHandler :
      RestHandlerTransform<'K, 'E, 'Req> ->
      RestHandler<'K, 'E> ->
      RestHandler<'K, 'E>
    )
    (transformHandler : RestHandlerTransform<'K, 'E, 'Req>)
    (transformOp : RestOperation)
    (resource : RestResource<'K, 'E>)
    =
    let mapHandler =
      applyTransformHandler transformHandler

    let mapOperations =
      RestOperationMap.add
        transformOp.Descriptions
        transformOp.Responses
        transformOp.Summary
        opType

    map mapHandler mapOperations resource

  let private withoutBuilder
    (operationType: RestOperations.RestOperationType)
    (handler: RestHandler<'K,'E> -> RestHandler<'K, 'E>)
    =
    map handler (Map.remove operationType)

  let withDelete transform =
    withBuilder RestOperations.Delete RestHandler.withDelete transform

  let withGet transform =
    withBuilder RestOperations.Get RestHandler.withGet transform

  let withPatch transform =
    withBuilder RestOperations.Patch RestHandler.withPatch transform

  let withPost transform =
    withBuilder RestOperations.Post RestHandler.withPost transform

  let withPut transform =
    withBuilder RestOperations.Put RestHandler.withPut transform

  let withQuery transform =
    withBuilder RestOperations.Query RestHandler.withQuery transform

  let withoutDelete resource =
    resource |> withoutBuilder RestOperations.Delete RestHandler.withoutDelete

  let withoutGet resource =
    resource |> withoutBuilder RestOperations.Get RestHandler.withoutGet

  let withoutPatch resource =
    resource |> withoutBuilder RestOperations.Patch RestHandler.withoutPatch

  let withoutPost resource =
    resource |> withoutBuilder RestOperations.Post RestHandler.withoutPost

  let withoutPut resource =
    resource |> withoutBuilder RestOperations.Put RestHandler.withoutPut

  let withoutQuery resource =
    resource |> withoutBuilder RestOperations.Query RestHandler.withoutQuery
