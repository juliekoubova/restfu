namespace Rest

module RestOperations =
  type RestOperationType =
  | Delete
  | Get
  | Post
  | Put
  | Query

type RestOperation = {
  Descriptions : string list
  Responses : RestResponse list
  Summary : string option
}

module RestOperation =
  let empty = {
    Descriptions = List.empty
    Responses = List.empty
    Summary = None
  }

  let map desc responses summary op = {
    Descriptions = desc op.Descriptions
    Responses = responses op.Responses
    Summary = summary op.Summary
  }


type RestOperationMap = Map<RestOperations.RestOperationType, RestOperation>

module RestOperationMap =

  let add descriptions responses summary opType table =
    Map.tryFind opType table
    |> Option.orElse (Some RestOperation.empty)
    |> Option.get
    |> RestOperation.map
       (fun prev -> List.append prev descriptions)
       (fun prev -> List.append prev responses)
       (fun prev -> Option.orElse prev summary)
    |> (fun op -> Map.add opType op table)

  let map fn =
    Map.map (fun (_, op) -> fn op)