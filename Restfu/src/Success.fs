namespace Restfu

type RestSuccess<'Result> = {
  Status : RestSuccessStatus
  Result : 'Result
}

module RestSuccess =
  let mapSuccess s r success = {
    Status = s success.Status
    Result = r success.Result
  }
  let mapSuccessResult f =
    mapSuccess id f
