namespace Rest

type RestResult<'Key, 'Entity> =
| DeleteSuccess of RestSuccessStatus * ('Key * 'Entity option)
| DeleteFail of RestFailDetails * 'Key

| GetSuccess of RestSuccessStatus * ('Key * 'Entity)
| GetFail of RestFailDetails * 'Key

| PatchSuccess of RestSuccessStatus * ('Key * JsonPatch * 'Entity option)
| PatchFail of RestFailDetails * ('Key * JsonPatch)

| PostSuccess of RestSuccessStatus * ('Key * 'Entity option)
| PostFail of RestFailDetails * 'Entity

| PutSuccess of RestSuccessStatus  * ('Key * 'Entity option)
| PutFail of RestFailDetails * ('Key * 'Entity)

| QuerySuccess of RestSuccessStatus * (RestQuery<'Entity> option * 'Entity seq)
| QueryFail of RestFailDetails * RestQuery<'Entity> option

module RestResult =

  let map k e p q fd result =
    let eOpt = Option.map e
    let qOpt = Option.map q
    let eSeq = Seq.map e
    let pairMap x y (a, b) =
      (x a, y b)
    let tripleMap x y z (a, b, c) =
      (x a, y b, z c)

    match result with
    | DeleteSuccess (status, result) ->
      DeleteSuccess (status, pairMap k eOpt result)

    | DeleteFail (details, key) ->
      DeleteFail (fd details, k key)

    | GetSuccess (status, result) ->
      GetSuccess (status, pairMap k e result)

    | GetFail (details, key) ->
      GetFail (fd details, k key)

    | PatchSuccess (status, result) ->
      PatchSuccess (status, (tripleMap k p eOpt result))

    | PatchFail (details, context) ->
      PatchFail (fd details, (pairMap k p context))

    | PostSuccess (status, result) ->
      PostSuccess (status, pairMap k eOpt result)

    | PostFail (details, entity) ->
      PostFail (fd details, e entity)

    | PutSuccess (success, result) ->
      PutSuccess (success, pairMap k eOpt result)

    | PutFail (details, context) ->
      PutFail (fd details, pairMap k e context)

    | QuerySuccess (status, result) ->
      QuerySuccess (status, pairMap qOpt eSeq result)

    | QueryFail (details, query) ->
      QueryFail (fd details, qOpt query)

  let mapKey k = map k id id id id
  let mapEntity e p q = map id e p q id
  let mapPatch p = map id id p id
  let mapQuery q = map id id id q id
  let mapFailDetails fd = map id id id id fd

