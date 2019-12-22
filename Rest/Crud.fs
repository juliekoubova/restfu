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

  withPut put [ cannotChangeKey () ]

let applyEntityKeyName entityName keyName resource =
  let replace = NaturalLanguage.replaceTokens (Map.ofList [
    ("Entity", Some entityName)
    ("Key", Some keyName)
  ])

  let rename (d : RestFailDetails) : RestFailDetails = {
    Description = replace d.Description
    Status = d.Status
    Title = replace d.Title
    Type = d.Type
  }
  { resource with EntityName = entityName; KeyName = keyName }
  |> mapHandler (fun handler -> handler >> RestResult.mapFailDetails rename)


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

  empty
  |> withDelete (ignoreArg0 delete) [
      deleteSuccess<'E> ()
      notFoundKey ()
    ]
  |> withGet (ignoreArg0 get) [
    getSuccess<'E> ()
    notFoundKey ()
  ]
  |> withPost (ignoreArg0 post) [
    created<'E> ()
    alreadyExists ()
  ]
  |> withPut (ignoreArg0 put) [
    putSuccess<'E> ()
    created<'E> ()
  ]
  |> withQuery (ignoreArg0 query) [
    querySuccess<'E> ()
  ]
  |> validatePutKey entityKey
  |> applyEntityKeyName entityName keyName
