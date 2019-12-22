namespace Rest

type RestResult<'Key, 'Entity> =
| DeleteSuccess of RestSuccessStatus * ('Key * 'Entity option)
| DeleteFail of RestFailDetails * 'Key

| GetSuccess of RestSuccessStatus * ('Key * 'Entity)
| GetFail of RestFailDetails * 'Key

| PostSuccess of RestSuccessStatus * ('Key * 'Entity option)
| PostFail of RestFailDetails * 'Entity

| PutSuccess of RestSuccessStatus  * ('Key * 'Entity option)
| PutFail of RestFailDetails * ('Key * 'Entity)

| QuerySuccess of RestSuccessStatus * (RestQuery<'Entity> * 'Entity seq)
| QueryFail of RestFailDetails * RestQuery<'Entity>

module RestResult =

  let map k e q fd result =
    let eOpt = Option.map e
    let eSeq = Seq.map e
    let pairMap x y (a, b) =
      (x a, y b)

    match result with
    | DeleteSuccess (status, result) ->
      DeleteSuccess (status, pairMap k eOpt result)

    | DeleteFail (details, key) ->
      DeleteFail (fd details, k key)

    | GetSuccess (status, result) ->
      GetSuccess (status, pairMap k e result)

    | GetFail (details, key) ->
      GetFail (fd details, k key)

    | PostSuccess (status, result) ->
      PostSuccess (status, pairMap k eOpt result)

    | PostFail (details, entity) ->
      PostFail (fd details, e entity)

    | PutSuccess (success, result) ->
      PutSuccess (success, pairMap k eOpt result)

    | PutFail (details, context) ->
      PutFail (fd details, pairMap k e context)

    | QuerySuccess (status, result) ->
      QuerySuccess (status, pairMap q eSeq result)

    | QueryFail (details, query) ->
      QueryFail (fd details, q query)

  let mapKey f = map f id id id
  let mapFailDetails f = map id id id f
  let mapEntity f q = map id f id q
  let mapQuery f = map id id f

