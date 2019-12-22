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

let create<'K, 'E>
  (keyName : string)
  (delete  : 'K -> RestResult<'K, 'E>)
  (get     : 'K -> RestResult<'K, 'E>)
  (post    : 'E -> RestResult<'K, 'E>)
  (put     : 'K * 'E -> RestResult<'K, 'E>)
  (query   : RestQuery -> RestResult<'K, 'E>)
  =
  { empty with KeyName = keyName }
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
