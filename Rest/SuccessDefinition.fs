namespace Rest
open System.Reflection

type RestSuccessDefinition =
  {
    ContentType : TypeInfo
    Status : RestSuccessStatus
    Title : string
  }
  interface IRestResponseDefinition with
    member this.ContentType with get () = this.ContentType
    member _.IsSuccess with get () = true
    member this.Status with get () = RestSuccess this.Status
    member this.Title with get () = this.Title

module RestSuccessDefinition =

  let applySuccess (definition : unit -> RestSuccessDefinition) result =
    let def = definition ()
    {
      Status = def.Status
      Result = result
    }

  let success<'E> status title = {
    ContentType = typeof<'E>.GetTypeInfo()
    Status = status
    Title = title
  }

  let created<'E> () =
    success<'E> Created "{Entity} was successfully created."

  let deleteSuccess<'E> () =
    success<'E> Ok "{Entity} with the specified {Key} was successfully deleted."

  let getSuccess<'E> () =
    success<'E> Ok "{Entity} with the specified {Key} was found."

  let putSuccess<'E> () =
    success<'E> Ok "{Entity} with the specified {Key} was successfully replaced."

  let querySuccess<'E> () =
    success<'E array> Ok "Successfully returned matching {Entity:plural}."