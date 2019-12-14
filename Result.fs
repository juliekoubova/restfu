namespace Rest

type RestResult<'Key, 'Entity> =
| DeleteSuccess of RestSuccess * 'Key * 'Entity option
| DeleteFail of RestFail * 'Key

| GetSuccess of RestSuccess * 'Key * 'Entity
| GetFail of RestFail * 'Key

| ListSuccess of RestSuccess * 'Entity seq
| ListFail of RestFail

| PostSuccess of RestSuccess * 'Key * 'Entity option
| PostFail of RestFail * 'Entity

| PutSuccess of RestSuccess * 'Key * 'Entity option
| PutFail of RestFail * 'Key * 'Entity

module RestResult =
  let map k e result =
    let eOpt = Option.map e
    let eSeq = Seq.map e
    match result with
    | DeleteSuccess (status, key, entity) ->
      DeleteSuccess (status, (k key), (eOpt entity))

    | DeleteFail (status, key) ->
      DeleteFail (status, (k key))

    | GetSuccess (status, key, entity) ->
      GetSuccess (status, (k key), (e entity))

    | GetFail (status, key) ->
      GetFail (status, (k key))

    | ListSuccess (status, entities) ->
      ListSuccess (status, (eSeq entities))

    | ListFail (status) ->
      ListFail (status)

    | PostSuccess (status, key, entity) ->
      PostSuccess (status, (k key), (eOpt entity))

    | PostFail (status, entity) ->
      PostFail (status, (e entity))

    | PutSuccess (status, key, entity) ->
      PutSuccess (status, (k key), (eOpt entity))

    | PutFail (status, key, entity) ->
      PutFail (status, (k key), (e entity))

  let mapKey f = map f id
  let mapEntity f = map id f