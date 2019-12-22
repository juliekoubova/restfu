module Rest.Crud
open Internal
open RestFailDefinition
open RestResource
open RestSuccessDefinition

let validatePutKey entityKey =
  let put handler (key, entity) =
    let keyFromEntity = entityKey entity
    if key = keyFromEntity then
      Put (key, entity) |> handler
    else
      PutFail (applyFail cannotChangeKey (key, keyFromEntity) (key, entity))

  withPut put {
    Descriptions = [
      "The {Key} of an existing {Entity} cannot be changed."
    ]
    Responses = [ failResponse cannotChangeKey ]
    Summary = None
  }


let create<'K, 'E when 'K : equality>
  (keyName   : string)
  (entityKey : 'E -> 'K)
  (delete    : 'K -> RestResult<'K, 'E>)
  (get       : 'K -> RestResult<'K, 'E>)
  (post      : 'E -> RestResult<'K, 'E>)
  (put       : 'K * 'E -> RestResult<'K, 'E>)
  (query     : RestQuery<'E> -> RestResult<'K, 'E>)
  =
  let entityName = typeof<'E>.Name

  { empty with EntityName = entityName; KeyName = keyName }
  |> withDelete (ignoreArg0 delete) {
    Descriptions = [
      "Deletes an existing {Entity} with the specified {Key}."
    ]
    Responses = [
      successResponse deleteOk
      failResponse notFoundKey
    ]
    Summary = Some "Delete {Entity} with the specified {Key}."
  }
  |> withGet (ignoreArg0 get) {
    Descriptions = [
      "Gets an existing {Entity} with the specified {Key}."
    ]
    Responses = [
      successResponse getOk
      failResponse notFoundKey
    ]
    Summary = Some "Get {Entity} with the specified {Key}."
  }
  |> withPost (ignoreArg0 post) {
    Descriptions = [
      "Creates a new {Entity}. Fails if an {Entity} with the same {Key} already exists."
    ]
    Responses = [
      successResponse postCreated
      failResponse alreadyExists
    ]
    Summary = Some "Create new {Entity}."
  }
  |> withPut (ignoreArg0 put) {
    Descriptions = [
      "Creates a new {Entity} or replaces an existing one with the same {Key}."
    ]
    Responses = [
      successResponse putCreated
      successResponse putOk
    ]
    Summary = Some "Create or replace {Entity} with the specified {Key}."
  }
  |> withQuery (ignoreArg0 query) {
    Descriptions = [
      "Finds all {Entity:plural} matching the specified query."
    ]
    Responses = [
      successResponse queryOk
    ]
    Summary = Some "Find all {Entity:plural} matching the query."
  }
  |> validatePutKey entityKey
