namespace Rest

type RestResult<'Id, 'Entity> =
| DeleteSuccess of Status * 'Id * 'Entity option
| DeleteFail of Status * 'Id

| GetSuccess of Status * 'Id * 'Entity
| GetFail of Status * 'Id

| ListSuccess of Status * 'Entity seq
| ListFail of Status

| PostSuccess of Status * 'Id * 'Entity option
| PostFail of Status * 'Entity

| PutSuccess of Status * 'Id * 'Entity option
| PutFail of Status * 'Id * 'Entity

module RestResult =
  let map (fId: 'IdA -> 'IdB) (fEntity: 'EntityA -> 'EntityB) result =
    let fEntityOpt = Option.map fEntity
    let fEntitySeq = Seq.map fEntity
    match result with
    | DeleteSuccess (status, id, entity) ->
      DeleteSuccess (status, (fId id), (fEntityOpt entity))

    | DeleteFail (status, id) ->
      DeleteFail (status, (fId id))

    | GetSuccess (status, id, entity) ->
      GetSuccess (status, (fId id), (fEntity entity))

    | GetFail (status, id) ->
      GetFail (status, (fId id))

    | ListSuccess (status, entities) ->
      ListSuccess (status, (fEntitySeq entities))

    | ListFail (status) ->
      ListFail (status)

    | PostSuccess (status, id, entity) ->
      PostSuccess (status, (fId id), (fEntityOpt entity))

    | PostFail (status, entity) ->
      PostFail (status, (fEntity entity))

    | PutSuccess (status, id, entity) ->
      PutSuccess (status, (fId id), (fEntityOpt entity))

    | PutFail (status, id, entity) ->
      PutFail (status, (fId id), (fEntity entity))

  let mapId f = map f id
  let mapEntity f = map id f