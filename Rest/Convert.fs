namespace Rest

module Convert =
  let key resource convert revert =
    resource |> RestResource.mapHandler (fun handler ->
      RestRequest.mapKey convert
      >> handler
      >> RestResult.mapKey revert
    )

  let entity resource convert revert =
    resource |> RestResource.mapHandler (fun handler ->
      RestRequest.mapEntity convert
      >> handler
      >> RestResult.mapEntity revert
    )
