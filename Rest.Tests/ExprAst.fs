module Rest.Tests.ExprAstTests
open Expecto
open Rest
open Rest.ExprAst

let bTrue = Value (Boolean true)
let bFalse = Value (Boolean false)
let prop = Value (Property ["Prop"])
let propX = Value (Property ["Prop"; "X"])
let weedNumber = Value (Number 420.0)
let sexNumber = Value (Number 69.0)
let onTheBackSeatOfACar = Value (Number 420.69)
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
    (Binary (GreaterThanOrEqual, prop, sexNumber), "(Prop ge 69)")
    (Binary (LessThan, prop, sexNumber), "(Prop lt 69)")
    (Binary (LessThanOrEqual, prop, sexNumber), "(Prop le 69)")
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
    testList "parse" [
      for (expected, str) in cases ->
        test (str.Replace (".", "\u2024")) {
          Expect.equal
            (parse str)
            (Result.Ok expected)
            "Unexpected parse result"
        }
    ]
  ]