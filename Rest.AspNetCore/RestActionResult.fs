module Rest.AspNetCore.RestActionResult
open Rest

open System
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc

let private successCode success =
  match success with
  | Ok -> StatusCodes.Status200OK
  | Created -> StatusCodes.Status201Created
  | Accepted -> StatusCodes.Status202Accepted
  | NoContent -> StatusCodes.Status204NoContent

let private failCode fail =
  match fail with
  | BadRequest -> StatusCodes.Status400BadRequest
  | Unauthorized -> StatusCodes.Status401Unauthorized
  | Forbidden -> StatusCodes.Status403Forbidden
  | NotFound -> StatusCodes.Status404NotFound
  | MethodNotAllowed -> StatusCodes.Status405MethodNotAllowed
  | Conflict -> StatusCodes.Status409Conflict
  | InternalServerError -> StatusCodes.Status500InternalServerError

let private problemDetails ``type`` title desc code =
  let result = ProblemDetails ()
  result.Detail <- desc
  result.Title <- title
  result.Type <- ``type``
  result.Status <- Nullable(int code)
  result

let private objectResult entity code : IActionResult =
  match entity with
  | None -> upcast StatusCodeResult (int code)
  | Some entity -> upcast ObjectResult (entity, StatusCode = Nullable(int code))

let fromResult<'K, 'E> (obj : obj) : IActionResult =
  match obj with
  | :? RestResult<'K, 'E> as result ->
    match result with
    | DeleteSuccess { Status = status; Result = (_, entity) } ->
      successCode status |> objectResult entity

    | GetSuccess { Status = status; Result = (_, entity) } ->
      successCode status |> objectResult (Some entity)

    | PostSuccess { Status = status; Result = (_, entity) } ->
      successCode status |> objectResult entity

    | PutSuccess { Status = status; Result = (_, entity) } ->
      successCode status |> objectResult entity

    | QuerySuccess { Status = status; Result = (_, entities) } ->
      successCode status |> objectResult (Some entities)

    | DeleteFail { Status = s; Type = t; Title = title; Description = desc }
    | GetFail { Status = s; Type = t; Title = title; Description = desc }
    | PostFail { Status = s; Type = t; Title = title; Description = desc }
    | PutFail { Status = s; Type = t; Title = title; Description = desc }
    | QueryFail { Status = s; Type = t; Title = title; Description = desc } ->
      let code = failCode s
      let details = problemDetails t title desc code
      objectResult (Some details) code

  | _ -> upcast StatusCodeResult(500)