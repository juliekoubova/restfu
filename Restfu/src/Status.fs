namespace Restfu
open System.Reflection

type RestSuccessStatus =
| Ok
| Created
| Accepted
| NoContent

type RestFailStatus =
| BadRequest
| Unauthorized
| Forbidden
| NotFound
| MethodNotAllowed
| Conflict
| InternalServerError

type RestStatus =
| RestSuccess of RestSuccessStatus
| RestFail of RestFailStatus
