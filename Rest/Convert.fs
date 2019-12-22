namespace Rest

module Convert =
  let key convert revert =
    RestResource.mapHandler (fun handler ->
      RestRequest.mapKey convert
      >> handler
      >> RestResult.mapKey revert
    )

  let entity convert convertQuery revert revertQuery =
    RestResource.mapHandler (fun handler ->
      RestRequest.mapEntity convert convertQuery
      >> handler
      >> RestResult.mapEntity revert revertQuery
    )
