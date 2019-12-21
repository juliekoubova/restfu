namespace Rest

type RestSuccessDefinition =
  {
    Status : RestSuccessStatus
    Title : string
  }
  interface IRestResponseDefinition with
    member this.Status with get () = RestSuccess this.Status
    member this.Title with get () = this.Title

module RestSuccessDefinition =

  let applySuccess (definition : unit -> RestSuccessDefinition) result =
    let def = definition ()
    {
      Status = def.Status
      Result = result
    }

  let success status title = {
    Status = status
    Title = title
  }

  let created () =
    success Created "{Entity} was successfully created."

  let deleteSuccess () =
    success Ok "{Entity} with the specified {Key} was successfully deleted."

  let getSuccess () =
    success Ok "{Entity} with the specified {Key} was found."

  let putSuccess () =
    success Ok "{Entity} with the specified {Key} was successfully replaced."

  let querySuccess () =
    success Ok "Successfully listed matching entities."