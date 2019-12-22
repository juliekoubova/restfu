namespace Rest
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

type IRestResponseDefinition =
  abstract member ContentType : TypeInfo with get
  abstract member IsSuccess : bool with get
  abstract member Status : RestStatus with get
  abstract member Title : string with get
