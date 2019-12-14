namespace Rest

type RestSuccess =
  | Ok
  | Created
  | Accepted
  | NoContent

type RestFail =
  | BadRequest of string
  | Unauthorized of string
  | Forbidden of string
  | NotFound of string
  | MethodNotAllowed of string
  | Conflict of string
  | InternalServerError of string

module RestFail =
  let alreadyExists key =
    sprintf "Entity with key %A already exists" key |> Conflict

  let cannotChangeKey uriKey entityKey =
    sprintf "Cannot change key from %A to %A" uriKey entityKey |> BadRequest

  let notFound key =
    sprintf "Entity with key %A not found" key |> NotFound