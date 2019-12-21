namespace Rest

type RestResult<'Key, 'Entity> =
| DeleteSuccess of RestSuccess * ('Key * 'Entity option)
| DeleteFail of RestFail * 'Key

| GetSuccess of RestSuccess * ('Key * 'Entity)
| GetFail of RestFail * 'Key

| ListSuccess of RestSuccess * (RestQuery * 'Entity seq)
| ListFail of RestFail * RestQuery

| PostSuccess of RestSuccess * ('Key * 'Entity option)
| PostFail of RestFail * 'Entity

| PutSuccess of RestSuccess * ('Key * 'Entity option)
| PutFail of RestFail * ('Key * 'Entity)

module RestResult =
  let map k e q result =
    let eOpt = Option.map e
    let eSeq = Seq.map e
    match result with
    | DeleteSuccess (success, (key, entity)) ->
      DeleteSuccess (success, ((k key), (eOpt entity)))

    | DeleteFail (fail, key) ->
      DeleteFail (fail, (k key))

    | GetSuccess (success, (key, entity)) ->
      GetSuccess (success, ((k key), (e entity)))

    | GetFail (fail, key) ->
      GetFail (fail, (k key))

    | ListSuccess (success, (query, entities)) ->
      ListSuccess (success, ((q query), (eSeq entities)))

    | ListFail (fail, query) ->
      ListFail (fail, (q query))

    | PostSuccess (success, (key, entity)) ->
      PostSuccess (success, ((k key), (eOpt entity)))

    | PostFail (fail, entity) ->
      PostFail (fail, (e entity))

    | PutSuccess (success, (key, entity)) ->
      PutSuccess (success, ((k key), (eOpt entity)))

    | PutFail (fail, (key, entity)) ->
      PutFail (fail, ((k key), (e entity)))

  let mapKey f = map f id id
  let mapEntity f = map id f id