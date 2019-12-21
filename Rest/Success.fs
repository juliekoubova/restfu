namespace Rest

type RestSuccess<'Result> = {
  Status : RestSuccessStatus
  Result : 'Result
}

module RestSuccess =
  let mapResult f success = {
    Status = success.Status
    Result = f success.Result
  }
