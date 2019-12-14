namespace Rest

module InMemory =
  let create (entityKey: 'Entity -> 'Key): RestResource<'Key, 'Entity> =
    let mutable state : Map<'Key, 'Entity> = Map.empty
    let tryFind key = Map.tryFind key state

    function
    | Delete key ->
        match tryFind key with
        | None -> DeleteFail ((RestFail.notFound key), key)
        | Some entity ->
          state <- Map.remove key state
          DeleteSuccess (Ok, key, Some entity)

    | Get key ->
        match tryFind key with
        | None -> GetFail ((RestFail.notFound key), key)
        | Some entity -> GetSuccess (Ok, key, entity)

    | List ->
        let entities = state |> Map.toSeq |> Seq.map snd
        ListSuccess (Ok, entities)

    | Post entity ->
        let key = entityKey entity
        match tryFind key with
        | Some _ -> PostFail (RestFail.alreadyExists key, entity)
        | None ->
          state <- Map.add key entity state
          PostSuccess (Created, key, Some entity)

    | Put (key, entity) ->
        state <- Map.add key entity state
        PutSuccess (Ok, key, Some entity)

