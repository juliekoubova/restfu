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

| QuerySuccess of RestSuccess<(RestQuery * 'Entity seq)>
| QueryFail of RestFail<RestQuery>

module RestResult =

  let map k e q result =
    let eOpt = Option.map e
    let eSeq = Seq.map e
    let pairMap x y (a, b) =
      (x a, y b)

    match result with
    | DeleteSuccess success ->
      DeleteSuccess (mapResult (pairMap k eOpt) success)

    | DeleteFail fail ->
      DeleteFail (mapContext k fail)

    | GetSuccess success ->
      GetSuccess (mapResult (pairMap k e) success)

    | GetFail fail ->
      GetFail (mapContext k fail)

    | PostSuccess success ->
      PostSuccess (mapResult (pairMap k eOpt) success)

    | PostFail fail ->
      PostFail (mapContext e fail)

    | PutSuccess success ->
      PutSuccess (mapResult (pairMap k eOpt) success)

    | PutFail fail ->
      PutFail (mapContext (pairMap k e) fail)

    | QuerySuccess success ->
      QuerySuccess (mapResult (pairMap q eSeq) success)

    | QueryFail fail ->
      QueryFail (mapContext q fail)

  let mapKey f = map f id id
  let mapEntity f = map id f id
