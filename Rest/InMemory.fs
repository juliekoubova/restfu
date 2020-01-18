namespace Rest
open RestFailDefinition
open RestSuccessDefinition
open Microsoft.FSharp.Quotations

module private InMemoryResource =
  let create (entityKey : EntityKey<'Key, 'Entity>) =

    let mutable state : Map<'Key, 'Entity> = Map.empty
    let tryFind key = Map.tryFind key state

    let delete key =
      match tryFind key with
      | None -> DeleteFail (applyFail notFoundKey key key)
      | Some entity ->
        state <- Map.remove key state
        DeleteSuccess (applySuccess deleteOk (key, Some entity))

    let get key =
      match tryFind key with
      | None -> GetFail (applyFail notFoundKey key key)
      | Some entity -> GetSuccess (applySuccess getOk (key, entity))

    let patch (key, patch) =
      match tryFind key with
      | None -> PatchFail (applyFail notFoundKey key (key, patch))
      | Some _ -> PatchFail (applyFail internalServerError () (key, patch))

    let post entity =
      let key = entityKey.Value entity
      match tryFind key with
      | Some _ -> PostFail (applyFail alreadyExists key entity)
      | None ->
        state <- Map.add key entity state
        PostSuccess (applySuccess postCreated (key, Some entity))

    let put (key, entity) =
      let success =
        match tryFind key with
        | Some _ -> putOk<'Entity>
        | None -> putCreated<'Entity>
      state <- Map.add key entity state
      PutSuccess (applySuccess success (key, Some entity))

    let query q =
      let entities =
        state
        |> Map.toSeq
        |> Seq.map snd
        |> RestQuery.apply q
      QuerySuccess (applySuccess queryOk (q, entities))

    Crud.create entityKey delete get patch post put query


[<AbstractClass; Sealed>]
type InMemory private () =

  static member Create<'Key, 'Entity when 'Key : comparison>
    ([<ReflectedDefinition(true)>] entityKey : Expr<'Entity -> 'Key>)
    : RestResource<'Key, 'Entity>
    =
    entityKey |> EntityKey.validate |> InMemoryResource.create