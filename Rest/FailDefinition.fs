namespace Rest
open System.Reflection

type RestFailDefinition<'Details> =
  {
    Status : RestFailStatus
    Type : string
    Title : string
    Description : 'Details -> string
  }
  interface IRestResponseDefinition with
    member this.ContentType with get () = typedefof<RestFail<_>>.GetTypeInfo ()
    member this.IsSuccess with get () = false
    member this.Status with get () = RestFail this.Status
    member this.Title with get () = this.Title

module RestFailDefinition =
  let applyFail (definition : unit -> RestFailDefinition<'a>) arg context =
    let def = definition ()
    {
      Details = {
        Description = def.Description arg
        Status = def.Status
        Title = def.Title
        Type = def.Type
      }
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
      "Cannot change {Entity:possessive} {Key}."
      (fun (uriKey, entityKey) ->
        sprintf "Cannot change {Entity:possessive} key from %A to %A." uriKey entityKey
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