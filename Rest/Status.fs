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
    Conflict <| sprintf "Entity with key %A already exists" key

  let cannotChangeKey uriKey entityKey =
    BadRequest <| sprintf "Cannot change key from %A to %A" uriKey entityKey

  let methodNotAllowed =
    MethodNotAllowed "Method Not Allowed"

  let notFound =
    NotFound "Not Found"

  let notFoundKey key =
    NotFound <| sprintf "Entity with key %A not found" key
