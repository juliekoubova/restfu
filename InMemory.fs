namespace Rest

module InMemory =
  let create (entityId: 'Entity -> 'Id): RestResource<'Id, 'Entity> =
    let mutable state : Map<'Id, 'Entity> = Map.empty
    let tryFind id = Map.tryFind id state

    function
    | Delete id ->
        match tryFind id with
        | None -> DeleteFail (NotFound, id)
        | Some entity ->
          state <- Map.remove id state
          DeleteSuccess (Ok, id, Some entity)

    | Get id ->
        match tryFind id with
        | None -> GetFail (NotFound, id)
        | Some entity -> GetSuccess (Ok, id, entity)

    | List ->
        let entities = state |> Map.toSeq |> Seq.map snd
        ListSuccess (Ok, entities)

    | Post entity ->
        let id = entityId entity
        match tryFind id with
        | Some _ -> PostFail (Conflict, entity)
        | None ->
          state <- Map.add id entity state
          PostSuccess (Created, id, Some entity)

    | Put (id, entity) ->
        state <- Map.add id entity state
        PutSuccess (Ok, id, Some entity)

