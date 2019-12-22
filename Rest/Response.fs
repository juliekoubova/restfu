namespace Rest
open System.Reflection

type RestResponse = {
  ContentType : (TypeInfo * TypeInfo) -> TypeInfo
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

