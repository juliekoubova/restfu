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
    ("not (F ne 1)", [1.0])
    ("F ne 1", [2.0; 10.0])
    ("F gt 2", [10.0])
    ("F ge 2", [2.0; 10.0])
    ("F lt 2", [1.0])
    ("F le 2", [1.0;2.0])
    ("(F lt 2) or (F gt 2)", [1.0;10.0])
    ("(F lt 2) and (F gt 2)", [])
  ]

[<Tests>]
let tests =
  testList "QueryEvaluator" [
    for (str, expected) in cases ->
      test (str.Replace (".", "\u2024")) {
        let result =
          RestExpr.parse<Entity> str |> Result.map (
            (fun expr -> { RestQuery.empty with Filter = Some expr }) >>
            (fun query -> RestQuery.apply query entities ) >>
            List.ofSeq
          )

        Expect.equal
          result
          (Result.Ok (expected |> List.map (fun f -> { F = f })))
          (sprintf "Unexpected filter result for '%s'" str)
      }

  ]