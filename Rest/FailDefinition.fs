namespace Rest
open System.Reflection

type RestFailDefinition<'Arg> =
  {
    ResponseType : RestResponse
    Format : 'Arg -> RestFailDetails
  }

module RestFailDefinition =

  [<Literal>]
  let private BaseUrl = "https://github.com/juliekoubova/tired-rest/wiki/Problems#"

  let applyFail definition arg context =
    (definition ()).Format arg, context

  let failResponse definition =
    (definition ()).ResponseType

  let fail<'Arg> status ``type`` summary (description : 'Arg -> string) =
    let namespacedType = BaseUrl + ``type``
    {
      ResponseType = {
        ContentType = fun _ -> typeof<RestFailDetails>.GetTypeInfo ()
        Status = RestFail status
        Summary = summary
      }
      Format = fun arg -> {
        Description = description arg
        Status = status
        Title = summary
        Type = namespacedType
      }
    }

  let alreadyExists () =
    fail
      Conflict
      "already-exists"
      "{Entity} with the specified {Key} already exists."
      (sprintf "{Entity} with key %A already exists.")

  let cannotChangeKey<'K> () =
    fail<'K * 'K>
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