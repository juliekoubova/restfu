namespace Rest
open RestFail
open RestSuccess

type RestResult<'Key, 'Entity> =
| DeleteSuccess of RestSuccess<('Key * 'Entity option)>
| DeleteFail of RestFail<'Key>

| GetSuccess of RestSuccess<('Key * 'Entity)>
| GetFail of RestFail<'Key>

| PostSuccess of RestSuccess<('Key * 'Entity option)>
| PostFail of RestFail<'Entity>

| PutSuccess of RestSuccess<('Key * 'Entity option)>
| PutFail of RestFail<('Key * 'Entity)>

| QuerySuccess of RestSuccess<(RestQuery<'Entity> * 'Entity seq)>
| QueryFail of RestFail<RestQuery<'Entity>>

module RestResult =

  let map k e q fd result =
    let eOpt = Option.map e
    let eSeq = Seq.map e
    let pairMap x y (a, b) =
      (x a, y b)

    match result with
    | DeleteSuccess success ->
      DeleteSuccess (mapSuccessResult (pairMap k eOpt) success)

    | DeleteFail fail ->
      DeleteFail (mapFail k fd fail)

    | GetSuccess success ->
      GetSuccess (mapSuccessResult (pairMap k e) success)

    | GetFail fail ->
      GetFail (mapFail k fd fail)

    | PostSuccess success ->
      PostSuccess (mapSuccessResult (pairMap k eOpt) success)

    | PostFail fail ->
      PostFail (mapFail e fd fail)

    | PutSuccess success ->
      PutSuccess (mapSuccessResult (pairMap k eOpt) success)

    | PutFail fail ->
      PutFail (mapFail (pairMap k e) fd fail)

    | QuerySuccess success ->
      QuerySuccess (mapSuccessResult (pairMap q eSeq) success)

    | QueryFail fail ->
      QueryFail (mapFail q fd fail)

  let mapKey f = map f id id id
  let mapFailDetails f = map id id id f
  let mapEntity f q = map id f id q
  let mapQuery f = map id id f

