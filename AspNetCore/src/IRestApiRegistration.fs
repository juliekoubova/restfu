namespace Restfu.AspNetCore
open Restfu

type IRestApiRegistration =
  abstract member Resource : IRestResource with get
  abstract member Url : string with get

type RestApiRegistration (url, resource) =
  interface IRestApiRegistration with
    member _.Resource with get () = resource
    member _.Url with get () = url