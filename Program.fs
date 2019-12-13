
open System

type LogEntryLevel = Error | Debug
type LogEntry =  LogEntryLevel * String

type Result<'T> =
  | Ok of 'T
  | Created of 'T
  | Accepted of 'T
  | NoContent
  | BadRequest of String
  | Unauthorized
  | Forbidden
  | NotFound
  | MethodNotAllowed
  | InternalServerError

type Response<'T> = {
  Log : LogEntry list
  Result : Result<'T>
}

module Response =
  let fromResult result = {
    Log = []
    Result = result
  }

type Request<'T> = {
  Body : 'T
}

type Handler<'Id,'Req,'Res> = 'Id -> Request<'Req> -> Response<'Res>

type RestSingleton<'T> = {
  Get : Handler<unit, unit, 'T>
  Put : Handler<unit, 'T, 'T>
}

type RestResource<'Id, 'Entity> = {
  Delete : Handler<'Id, unit, 'Entity>
  List   : Handler<unit, unit,  'Entity seq>
  Get    : Handler<'Id, unit, 'Entity>
  Post   : Handler<unit, 'Entity, 'Entity>
  Put    : Handler<'Id, 'Entity, 'Entity>
}

let notFound _ _ = { Result = NotFound; Log = [] }

let emptyResource = {
  Delete = notFound
  List   = notFound
  Get    = notFound
  Post   = notFound
  Put    = notFound
}

let inMemoryCollection
  (id: 'Entity -> 'Id)
  =
  let mutable state = Map.empty

  let get id =
    match Map.tryFind id state with
    | Some entity -> Ok entity
    | None -> NotFound

  {
    Delete = fun id _ ->
      match get id with
      | Ok entity ->
        state <- Map.remove id state
        Response.fromResult(Ok entity)
      | other -> other |> Response.fromResult

    Get = fun id _ -> get id |> Response.fromResult

    List = fun _ _ ->
      state
      |> Map.toSeq
      |> Seq.map snd
      |> Ok
      |> Response.fromResult

    Post = fun _ { Body = entity } ->
      state <- Map.add (id entity) entity state
      Ok entity |> Response.fromResult

    Put = fun id { Body = entity } ->
      state <- Map.add id entity state
      Ok entity |> Response.fromResult
  }

let validatePutId
  (idSelector: 'Entity -> 'Id)
  (resource: RestResource<'Id, 'Entity>)
  =
  { resource with
      Put = fun id req ->
        if id = idSelector(req.Body) then
          resource.Put id req
        else
          BadRequest "Id doesn't match" |> Response.fromResult
  }

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    0 // return an integer exit code