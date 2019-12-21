namespace Rest

type RestFail<'Context> = {
  Status : RestFailStatus
  Type : string
  Title : string
  Description : string
  Context : 'Context
}

module RestFail =
  let mapContext f fail = {
    Status = fail.Status
    Type = fail.Type
    Title = fail.Title
    Description = fail.Description
    Context = f fail.Context
  }
