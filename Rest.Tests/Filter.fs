module Rest.Tests.FilterTests
open Expecto
open Rest
open Rest.RestFilter

type T = { Prop : obj }
let bTrue = Value (Boolean true)
let bFalse = Value (Boolean false)
let prop = Value (Property (<@ fun (x : T) -> x.Prop @>))
let weedNumber = Value (Number 420m)
let sexNumber = Value (Number 69m)
let onTheBackSeatOfACar = Value (Number 420.69m)
let noice = Value (String "noice")

[<Tests>]
let tests =
  let cases = ([
    (bTrue, "true")
    (bFalse, "false")
    (onTheBackSeatOfACar, "420.69")
    (noice, "'noice'")
    (prop, "Prop")
    (Unary (Not, bFalse), "(not false)")
    (Binary (Equal, prop, weedNumber), "(Prop eq 420)")
    (Binary (NotEqual, sexNumber, noice), "(69 ne 'noice')")
    (Binary (GreaterThan, prop, sexNumber), "(Prop gt 69)")
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
  testList "RestFilter" [
    for (expr, expected) in cases ->
      test (sprintf "Serialize %s" (expected.Replace (".", "\u2024"))) {
        Expect.equal
          (serialize expr)
          expected
          "Unexpected serializer output"
      }
  ]