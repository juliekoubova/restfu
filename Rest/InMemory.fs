namespace Rest
open RestFail
open RestResource

module InMemory =
  let create (entityKey: 'Entity -> 'Key): RestResource<'Key, 'Entity> =
    let mutable state : Map<'Key, 'Entity> = Map.empty
    let tryFind key = Map.tryFind key state

    let delete key =
      match tryFind key with
      | None -> DeleteFail ((notFoundKey key), key)
      | Some entity ->
        state <- Map.remove key state
        DeleteSuccess (Ok, (key, Some entity))

    let get key =
      match tryFind key with
      | None -> GetFail ((notFoundKey key), key)
      | Some entity -> GetSuccess (Ok, (key, entity))

    let list query =
      let entities = state |> Map.toSeq |> Seq.map snd
      ListSuccess (Ok, (query, entities))

    let post entity =
      let key = entityKey entity
      match tryFind key with
      | Some _ -> PostFail (alreadyExists key, entity)
      | None ->
        state <- Map.add key entity state
        PostSuccess (Created, (key, Some entity))

    let put (key, entity) =
      state <- Map.add key entity state
      PutSuccess (Ok, (key, Some entity))

    create delete get list post put