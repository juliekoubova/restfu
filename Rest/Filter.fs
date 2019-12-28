namespace Rest
open FParsec

type RestFilterValue =
| Boolean of bool
| Number of float
| Property of string list
| String of string

type RestFilterBinaryOperator =
| Equal
| NotEqual
| GreaterThan
| GreaterThanOrEqual
| LessThan
| LessThanOrEqual
| And
| Or

type RestFilterUnaryOperator =
| Not

type RestFilterExpr =
| Value of RestFilterValue
| Binary of RestFilterBinaryOperator * RestFilterExpr * RestFilterExpr
| Unary of RestFilterUnaryOperator * RestFilterExpr

module RestFilterParser =
  type ValueParser = Parser<RestFilterValue, unit>
  type ExprParser = Parser<RestFilterExpr, unit>

  let parseBoolean : ValueParser =
    stringReturn "true" (Boolean true) <|>
    stringReturn "false" (Boolean false) <?>
    "boolean"

  let parseNumber : ValueParser =
    pfloat <?> "number" |>> Number

  let parseString : ValueParser =
    StringLiteral.parse |>> String

  let parseProperty : ValueParser =
    // TODO : support full JSON Pointer maybe?
    let options = IdentifierOptions (label = "property path")
    sepBy1 (identifier options) (pchar '/') |>> Property

  let parseValue : ExprParser =
    choice [
      parseBoolean
      parseNumber
      parseString
      parseProperty
    ] |>> Value

  let parseExpr, parseExprRef = createParserForwardedToRef ()

  let parseValueOrSubExpr : ExprParser =
    choice [
      between (pchar '(') (pchar ')') parseExpr
      parseValue
    ]

  let unaryOp s op p =
    PrefixOperator<RestFilterExpr, unit, unit> (
      s,
      notFollowedBy letter >>. spaces,
      p, true,
      fun term -> Unary (op, term)
    )

  let binaryOp s op p =
    InfixOperator<RestFilterExpr, unit, unit> (
      s,
      notFollowedBy letter >>. spaces,
      p, Associativity.Left,
      fun left right -> Binary (op, left, right)
    )

  let opp = OperatorPrecedenceParser<RestFilterExpr, unit, unit> ()
  opp.AddOperator (unaryOp "not" Not 100)
  opp.AddOperator (binaryOp "gt" GreaterThan 90)
  opp.AddOperator (binaryOp "ge" GreaterThanOrEqual 90)
  opp.AddOperator (binaryOp "lt" LessThan 90)
  opp.AddOperator (binaryOp "le" LessThanOrEqual 90)
  opp.AddOperator (binaryOp "eq" Equal 80)
  opp.AddOperator (binaryOp "ne" NotEqual 80)
  opp.AddOperator (binaryOp "and" And 70)
  opp.AddOperator (binaryOp "or" Or 60)

  opp.TermParser <- parseValueOrSubExpr .>> spaces
  parseExprRef := opp.ExpressionParser

  let parse str =
    run parseExpr str

module RestFilter =

  let rec foldBack fValue fBinary fUnary expr continuation: 'r =
    let recurse = foldBack fValue fBinary fUnary
    match expr with
    | Value value -> continuation (fValue value)

    | Binary (operator, left, right) ->
      recurse left (fun left ->
        recurse right (fun right ->
          continuation (fBinary operator left right)
        )
      )

    | Unary (operator, operand) ->
      recurse operand (fun operand -> continuation (fUnary operator operand))

  let parse str =
    match RestFilterParser.parse str with
    | Success (result, _, _) -> Result.Ok result
    | Failure (message, _, _) -> Result.Error message

  let serialize filter =
    let serializeValue value =
      match value with
      | Boolean true -> "true"
      | Boolean false -> "false"
      | Number num -> sprintf "%g" num
      | Property path -> path |> String.concat "/"
      | String str -> sprintf "'%s'" str

    let serializeUnary unary operand =
      let operator =
        match unary with
        | Not -> "not"
      sprintf "(%s %s)" operator operand

    let serializeBinary binary left right =
      let operator =
        match binary with
        | Equal -> "eq"
        | NotEqual -> "ne"
        | LessThan -> "lt"
        | LessThanOrEqual -> "le"
        | GreaterThan -> "gt"
        | GreaterThanOrEqual -> "ge"
        | And -> "and"
        | Or -> "or"

      sprintf "(%s %s %s)" left operator right

    foldBack serializeValue serializeBinary serializeUnary filter id