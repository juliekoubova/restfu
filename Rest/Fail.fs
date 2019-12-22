namespace Rest

type RestFailDetails = {
  Status : RestFailStatus
  Type : string
  Title : string
  Description : string
}

type RestFail<'Context> = {
  Context : 'Context
  Details : RestFailDetails
}

module RestFail =
  let mapFail context details fail = {
    Context = context fail.Context
    Details = details fail.Details
  }
