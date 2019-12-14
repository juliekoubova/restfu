namespace Rest

module Convert =
  let key resource convert revert: RestResource<'Key, 'Entity> =
   fun req ->
      req
      |> RestRequest.mapKey convert
      |> resource
      |> RestResult.mapKey revert

  let entity resource convert revert: RestResource<'Key, 'Entity> =
   fun req ->
      req
      |> RestRequest.mapEntity convert
      |> resource
      |> RestResult.mapEntity revert
