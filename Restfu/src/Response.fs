namespace Restfu
open System

type RestResponse = {
  ContentType : (Type * Type) -> Type
  Status : RestStatus
  Summary : string
}

module RestResponse =

  let map ct st su response = {
    ContentType = ct response.ContentType
    Status = st response.Status
    Summary = su response.Summary
  }

  let mapSummary f =
    map id id f

