namespace Rest

module Convert =
  let key convert revert =
    RestResource.mapHandler (fun handler ->
      RestRequest.mapKey convert
      >> handler
      >> RestResult.mapKey revert
    )

  let entity convert revert =
    RestResource.mapHandler (fun handler ->
      RestRequest.mapEntity convert
      >> handler
      >> RestResult.mapEntity revert
    )
