module Rest.Tests.ExprTests
open Expecto
open Rest.ExprAst
open Rest


type P = {
  X : float32
}

type Entity = {
  Prop : P
}

let bTrue : RestExpr<Entity> = Literal true
let bFalse : RestExpr<Entity> = Literal false
let prop : RestExpr<Entity> = Property [typeof<Entity>.GetProperty "Prop"]
let propX : RestExpr<Entity> = Property [
    typeof<Entity>.GetProperty "Prop"
    typeof<P>.GetProperty "X"
  ]
let weedNumber : RestExpr<Entity> = Literal 420.0
let sexNumber : RestExpr<Entity> = Literal 69.0
let onTheBackSeatOfACar : RestExpr<Entity> = Literal 420.69
let noice : RestExpr<Entity> = Literal "noice"

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
    (
      Binary (
        GreaterThanOrEqual,
        Convert (typeof<float>, prop),
        sexNumber
      ),
      "(Prop ge 69)"
    )
    (
      Binary (
        LessThan,
        prop,
        sexNumber
      ),
      "(Prop lt 69)"
    )
    (Binary (LessThanOrEqual, prop, sexNumber), "(Prop le 69)")
    (
      Binary (
        Or,
        Binary (LessThan, Convert (typeof<float>, prop), sexNumber),
        Binary (GreaterThan, Convert (typeof<float>, prop), weedNumber)
      ),
      "((Prop lt 69) or (Prop gt 420))"
    )
  ])

  testList "RestFilter" [

    ftest "validateProperty" {
      let p = [ "Prop"; "X" ]
      Expect.equal
        (RestExpr.validatePropertyPath typeof<Entity> p)
        (Result.Ok [ typeof<Entity>.GetProperty "Prop"; typeof<P>.GetProperty "X" ])
        "Unexpected property path result"
    }

    testList "parse" [
      for (expected, str) in cases ->
        test (str.Replace (".", "\u2024")) {
          Expect.equal
            (RestExpr.parse typeof<Entity> str)
            (Result.Ok expected)
            "Unexpected parse result"
        }
    ]
    testList "serialize" [
      for (expr, expected) in cases ->
        test (expected.Replace (".", "\u2024")) {
          Expect.equal
            (RestExpr.serialize expr)
            expected
            "Unexpected serialize result"
        }
    ]
  ]
