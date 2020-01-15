module Rest.Tests.ExprTests
open Expecto
open Rest.ExprAst
open Rest


type P = {
  X : float
}

type Entity = {
  Prop : P
  I : int32
}

let bTrue : RestExpr<Entity> = Literal true
let bFalse : RestExpr<Entity> = Literal false
let i : RestExpr<Entity> = Property [typeof<Entity>.GetProperty "I"]
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
  let parseCases = ([
    (Result.Ok bTrue, "true")
    (Result.Ok bFalse, "false")
    (Result.Ok onTheBackSeatOfACar, "420.69")
    (Result.Ok noice, "'noice'")
    (Result.Ok prop, "Prop")
    (Result.Ok propX, "Prop/X")
    (Result.Ok (Unary (Not, bFalse)), "(not false)")
    (Result.Ok (Binary (Equal, i, (Literal 420))), "(I eq 420)")
    (
      Result.Error "'noice' : This expression was expected to have type 'Int32' but here has type 'String'",
      "(69 ne 'noice')"
    )
    (Result.Ok (Binary (GreaterThan, propX, sexNumber)), "(Prop/X gt 69.0)")
    (
      (Result.Ok (Binary
        (
          Or,
          Binary (LessThan, propX, sexNumber),
          Binary (GreaterThan, propX, weedNumber)
        ))
      ),
      "Prop/X lt 69.0 or Prop/X gt 420.0"
    )
  ])

  let serializeCases = [
    (bTrue, "true")
    (bFalse, "false")
    (onTheBackSeatOfACar, "420.69")
    (noice, "'noice'")
    (prop, "Prop")
    (propX, "Prop/X")
    (Unary (Not, bFalse), "(not false)")
    (Binary (Equal, i, weedNumber), "(I eq 420)")
    (Binary (NotEqual, sexNumber, noice), "(69 ne 'noice')")
    (Binary (GreaterThan, propX, sexNumber), "(Prop/X gt 69)")
  ]

  testList "RestFilter" [

    test "validateProperty" {
      let p = [ "Prop"; "X" ]
      Expect.equal
        (RestExpr.validatePropertyPath typeof<Entity> p)
        (Result.Ok [ typeof<Entity>.GetProperty "Prop"; typeof<P>.GetProperty "X" ])
        "Unexpected property path result"
    }

    testList "parse" [
      for (expected, str) in parseCases ->
        test (str.Replace (".", "\u2024")) {
          Expect.equal
            (RestExpr.parse<Entity> str)
            expected
            "Unexpected parse result"
        }
    ]

    testList "serialize" [
      for (expr, expected) in serializeCases ->
        test (expected.Replace (".", "\u2024")) {
          Expect.equal
            (RestExpr.serialize expr)
            expected
            "Unexpected serialize result"
        }
    ]
  ]
