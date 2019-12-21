namespace Rest
open RestFail
open RestFailDefinition
open RestSuccessDefinition

module InMemory =
  let create (entityKey: 'Entity -> 'Key): RestResource<'Key, 'Entity> =
    let mutable state : Map<'Key, 'Entity> = Map.empty
    let tryFind key = Map.tryFind key state

    let delete key =
      match tryFind key with
      | None -> DeleteFail (applyFail notFoundKey key key)
      | Some entity ->
        state <- Map.remove key state
        DeleteSuccess (applySuccess deleteSuccess (key, Some entity))

    let get key =
      match tryFind key with
      | None -> GetFail (applyFail notFoundKey key key)
      | Some entity -> GetSuccess (applySuccess getSuccess (key, entity))

    let post entity =
      let key = entityKey entity
      match tryFind key with
      | Some _ -> PostFail (applyFail alreadyExists key entity)
      | None ->
        state <- Map.add key entity state
        PostSuccess (applySuccess created (key, Some entity))

    let put (key, entity) =
      let success =
        match tryFind key with
        | Some _ -> putSuccess
        | None -> created
      state <- Map.add key entity state
      PutSuccess (applySuccess success (key, Some entity))

    let query q =
      let entities = state |> Map.toSeq |> Seq.map snd
      QuerySuccess (applySuccess querySuccess (q, entities))

    Crud.create delete get post put query