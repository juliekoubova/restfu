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
  | DeleteSuccess (status, (_, entity))
  | PatchSuccess (status, (_, _, entity))
  | PostSuccess (status, (_, entity))
  | PutSuccess (status, (_, entity)) ->
    successCode status |> objectResult entity

  | GetSuccess (status, (_, entity)) ->
    successCode status |> objectResult (Some entity)

  | QuerySuccess (status, (_, entities)) ->
    successCode status |> objectResult (Some entities)

  | DeleteFail (details, _)
  | GetFail (details, _)
  | PatchFail (details, _)
  | PostFail (details, _)
  | PutFail (details, _)
  | QueryFail (details, _) ->
    let code = failCode details.Status
    let details = problemDetails details.Type details.Title details.Description code
    objectResult (Some details) code