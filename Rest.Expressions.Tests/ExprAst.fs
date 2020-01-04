module Rest.Tests.ExprAstTests
open Expecto
open Rest
open Rest.ExprAst
open FParsec

let bTrue = Value (Boolean true)
let bFalse = Value (Boolean false)
let prop = Value (Property ["Prop"])
let propX = Value (Property ["Prop"; "X"])
let weedNumber = Value (Int32 420)
let sexNumber = Value (Int32 69)
let onTheBackSeatOfACar = Value (Float 420.69)
let noice = Value (String "noice")

[<Tests>]
let tests =
  let cases = ([
    (bTrue, "true")
    (bFalse, "false")
    (onTheBackSeatOfACar, "420.69")
    (noice, "'noice'")
    (prop, "Prop")
    (propX, "Prop/X")
    (Unary (Not, bFalse), "(not false)")
    (Binary (Equal, prop, weedNumber), "(Prop eq 420)")
    (Binary (NotEqual, sexNumber, noice), "(69 ne 'noice')")
    (Binary (GreaterThan, propX, sexNumber), "(Prop/X gt 69)")
    (Binary (GreaterThanOrEqual, prop, sexNumber), "Prop ge 69")
    (Binary (LessThan, prop, sexNumber), "Prop lt 69")
    (Binary (LessThanOrEqual, prop, sexNumber), "Prop le 69")
    (
      Binary (
        Or,
        Binary (LessThan, prop, sexNumber),
        Binary (GreaterThan, prop, weedNumber)
      ),
      "((Prop lt 69) or (Prop gt 420))"
    )
  ])

  testList "RestExprAst" [
    testList "parseNumber" [
      let numPairs = [
        ("42", Int32 42)
        ("42.0", Float 42.0)
        ("5000000000", Int64 5000000000L)
      ]
      for (str, expected) in numPairs ->
        testCase (str.Replace (".", "\u2024")) <| (fun _ ->
            match run parseNumber str with
            | Success (x, _, _) -> Expect.equal x expected "Unexpected result"
            | Failure (r, _, _) -> failwithf "%A" r
        )

    ]
    testList "parse" [
      for (expected, str) in cases ->
        testCase (str.Replace (".", "\u2024")) <| (fun _ ->
          Expect.equal
            (ExprAst.parse str)
            (Result.Ok expected)
            "Unexpected parse result"
        )
    ]
  ]