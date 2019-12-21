namespace Rest

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
  abstract member Status : RestStatus with get
  abstract member Title : string with get
