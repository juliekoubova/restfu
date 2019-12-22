module Rest.RestApiExplorer

let private modifyHelpStrings op =
  RestOperationMap

let applyEntityKeyName resource =
  let replace = NaturalLanguage.replaceTokens (Map.ofList [
    ("Entity", Some resource.EntityName)
    ("Key", Some resource.KeyName)
  ])

  let applyToFailDetails (details : RestFailDetails) : RestFailDetails = {
    Description = replace details.Description
    Status = details.Status
    Title = replace details.Title
    Type = details.Type
  }

  let applyToOperation (op : RestOperation) : RestOperation =
    RestOperation.map
      (List.map replace)
      (List.map (RestResponse.mapSummary replace))
      (Option.map replace)
      op

  resource
  |> RestResource.map
    (fun handler -> handler >> RestResult.mapFailDetails applyToFailDetails)
    (Map.map (fun _ op -> applyToOperation op))
