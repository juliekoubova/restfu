module Rest.AspNetCore.RestActionResult
open Rest
open Rest.AspNetCore.StatusCode

open System
open Microsoft.AspNetCore.Mvc

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

let fromResult<'K, 'E> (result : RestResult<'K, 'E>) : IActionResult =
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

  | DeleteFail { Details = details }
  | GetFail { Details = details }
  | PostFail { Details = details }
  | PutFail { Details = details }
  | QueryFail { Details = details } ->
    let code = failCode details.Status
    let details = problemDetails details.Type details.Title details.Description code
    objectResult (Some details) code