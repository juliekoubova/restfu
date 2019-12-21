module Rest.Crud
open Internal
open RestFailDefinition
open RestResource
open RestSuccessDefinition

let create delete get post put query =
  empty
  |> withDelete (ignoreArg0 delete) [
      deleteSuccess ()
      notFoundKey ()
    ]
  |> withGet (ignoreArg0 get) [
    getSuccess ()
    notFoundKey ()
  ]
  |> withPost (ignoreArg0 post) [
    created ()
    alreadyExists ()
  ]
  |> withPut (ignoreArg0 put) [
    putSuccess ()
    created ()
  ]
  |> withQuery (ignoreArg0 query) [
    querySuccess ()
  ]
