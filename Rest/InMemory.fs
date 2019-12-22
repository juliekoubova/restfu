namespace Rest
open RestFailDefinition
open RestSuccessDefinition
open Quotations

open FSharp.Quotations.Evaluator

module InMemory =
  let create (entityKeyExpr: Expr<'Entity -> 'Key>): RestResource<'Key, 'Entity> =

    let mutable state : Map<'Key, 'Entity> = Map.empty

    let entityKeyName = propertyName entityKeyExpr
    let entityKey = entityKeyExpr.Compile ()
    let tryFind key = Map.tryFind key state

    let delete key =
      match tryFind key with
      | None -> DeleteFail (applyFail notFoundKey key key)
      | Some entity ->
        state <- Map.remove key state
        DeleteSuccess (applySuccess deleteSuccess<'Entity> (key, Some entity))

    let get key =
      match tryFind key with
      | None -> GetFail (applyFail notFoundKey key key)
      | Some entity -> GetSuccess (applySuccess getSuccess<'Entity> (key, entity))

    let post entity =
      let key = entityKey entity
      match tryFind key with
      | Some _ -> PostFail (applyFail alreadyExists key entity)
      | None ->
        state <- Map.add key entity state
        PostSuccess (applySuccess created<'Entity> (key, Some entity))

    let put (key, entity) =
      let success =
        match tryFind key with
        | Some _ -> putSuccess<'Entity>
        | None -> created<'Entity>
      state <- Map.add key entity state
      PutSuccess (applySuccess success (key, Some entity))

    let query q =
      let entities = state |> Map.toSeq |> Seq.map snd
      QuerySuccess (applySuccess querySuccess<'Entity> (q, entities))

    Crud.create entityKeyName delete get post put query
    |> Crud.validatePutKey entityKey