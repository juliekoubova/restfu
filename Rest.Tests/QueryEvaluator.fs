module Rest.Tests.QueryEvaluatorTests
open Expecto
open Rest

type Entity = {
  F : float
}

let entities = [
  { F = 1.0 }
  { F = 2.0 }
  { F = 10.0 }
]

let cases : list<string * float list> =
  [
    ("F eq 1", [1.0])
    ("F ne 1", [2.0; 10.0])
  ]

[<Tests>]
let tests =
  testList "QueryEvaluator" [
    for (str, expected) in cases ->
      test (str.Replace (".", "\u2024")) {
        let result =
          RestExpr.parse<Entity> str |> Result.map (
            (fun expr -> { RestQuery.empty with Filter = Some expr }) >>
            (fun query -> QueryEvaluator.applyFilter query entities )
          )

        Expect.equal
          result
          (Result.Ok (expected |> Seq.map (fun f -> { F = f })))
          (sprintf "Unexpected filter result for '%s'" str)
      }

  ]