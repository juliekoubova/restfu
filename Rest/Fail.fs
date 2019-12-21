namespace Rest

type RestFailDefinition<'Details> =
  {
    Status : RestFailStatus
    Type : string
    Title : string
    Description : 'Details -> string
  }
  interface IRestResponseDefinition with
    member this.Status with get () = RestFail this.Status
    member this.Title with get () = this.Title

type RestFail<'Context> = {
  Status : RestFailStatus
  Type : string
  Title : string
  Description : string
  Context : 'Context
}

module RestFail =
  let mapContext f fail = {
    Status = fail.Status
    Type = fail.Type
    Title = fail.Title
    Description = fail.Description
    Context = f fail.Context
  }

module RestFailDefinition =
  let applyFail (definition : unit -> RestFailDefinition<'a>) arg context =
    let def = definition ()
    {
      Status = def.Status
      Type = def.Type
      Title = def.Title
      Description = def.Description arg
      Context = context
    }

  [<Literal>]
  let private BaseUrl = "https://github.com/juliekoubova/tired-rest/wiki/Problems#"

  let fail<'Arg> status ``type`` title (description : 'Arg -> string) = {
    Status = status
    Type = BaseUrl + ``type``
    Title = title
    Description = description
  }

  let alreadyExists () =
    fail
      Conflict
      "already-exists"
      "{Entity} with the specified {Key} already exists."
      (sprintf "{Entity} with key %A already exists.")

  let cannotChangeKey<'Key> () =
    fail<'Key * 'Key>
      BadRequest
      "cannot-change-key"
      "Cannot change {Entity} {Key}."
      (fun (uriKey, entityKey) ->
        sprintf "Cannot change key from %A to %A." uriKey entityKey
      )

  let methodNotAllowed () =
    fail
      MethodNotAllowed
      "method-not-allowed"
      "Resource doesn't support the specified method."
      (fun () -> "Method not allowed.")

  let notFound () =
    fail
      NotFound
      "not-found"
      "Not found"
      (fun () -> "Not found")

  let notFoundKey () =
    fail
      NotFound
      "key-not-found"
      "{Entity} with the specified {Key} could not be found."
      (sprintf "{Entity} with {Key} %A could not be found.")
