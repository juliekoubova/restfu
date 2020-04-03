module Restfu.AspNetCore.StatusCode
open Restfu
open Microsoft.AspNetCore.Http

let successCode success =
  match success with
  | Ok -> StatusCodes.Status200OK
  | Created -> StatusCodes.Status201Created
  | Accepted -> StatusCodes.Status202Accepted
  | NoContent -> StatusCodes.Status204NoContent

let failCode fail =
  match fail with
  | BadRequest -> StatusCodes.Status400BadRequest
  | Unauthorized -> StatusCodes.Status401Unauthorized
  | Forbidden -> StatusCodes.Status403Forbidden
  | NotFound -> StatusCodes.Status404NotFound
  | MethodNotAllowed -> StatusCodes.Status405MethodNotAllowed
  | Conflict -> StatusCodes.Status409Conflict
  | InternalServerError -> StatusCodes.Status500InternalServerError

let code status =
  match status with
  | RestSuccess success -> successCode success
  | RestFail fail -> failCode fail