module Rest.AspNetCore.RestActionResult

open System
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open Rest

let private successCode success =
  match success with
  | Ok -> StatusCodes.Status200OK
  | Created -> StatusCodes.Status201Created
  | Accepted -> StatusCodes.Status202Accepted
  | NoContent -> StatusCodes.Status204NoContent

let private failCodeMessage fail =
  match fail with
  | BadRequest message -> (StatusCodes.Status400BadRequest, message)
  | Unauthorized message -> (StatusCodes.Status401Unauthorized, message)
  | Forbidden message -> (StatusCodes.Status403Forbidden, message)
  | NotFound message -> (StatusCodes.Status404NotFound, message)
  | MethodNotAllowed message -> (StatusCodes.Status405MethodNotAllowed, message)
  | Conflict message -> (StatusCodes.Status409Conflict, message)
  | InternalServerError message -> (StatusCodes.Status500InternalServerError, message)

let private contentResult message code : IActionResult =
  upcast ContentResult (Content = message, StatusCode = Nullable(int code))

let private objectResult entity code : IActionResult =
  match entity with
  | None -> upcast StatusCodeResult (int code)
  | Some entity -> upcast ObjectResult (entity, StatusCode = Nullable(int code))

let fromResult<'K, 'E> (obj : obj) : IActionResult =
  match obj with
  | :? RestResult<'K, 'E> as result ->
    match result with
    | DeleteSuccess (success, (_, entity)) ->
      successCode success |> objectResult entity

    | GetSuccess (success, (_, entity)) ->
      successCode success |> objectResult (Some entity)

    | ListSuccess (success, (_, entities)) ->
      successCode success |> objectResult (Some entities)

    | PostSuccess (success, (_, entity)) ->
      successCode success |> objectResult entity

    | PutSuccess (success, (_, entity)) ->
      successCode success |> objectResult entity

    | DeleteFail (fail, _)
    | GetFail (fail, _)
    | ListFail (fail, _)
    | PostFail (fail, _)
    | PutFail (fail, _) ->
      let (code, message) = failCodeMessage fail
      contentResult message code

  | _ -> upcast StatusCodeResult(500)