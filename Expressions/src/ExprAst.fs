module Restfu.ExprAst
open FParsec

type Value =
| Boolean of bool
| Float of float
| Int32 of int32
| Int64 of int64
| Property of string list
| String of string

type BinaryOperator =
| Equal
| NotEqual
| GreaterThan
| GreaterThanOrEqual
| LessThan
| LessThanOrEqual
| And
| Or

type UnaryOperator =
| Not

type Expr =
| Value of Value
| Binary of BinaryOperator * Expr * Expr
| Unary of UnaryOperator * Expr

type private ValueParser = Parser<Value, unit>
type private ExprParser = Parser<Expr, unit>

let rec fold fValue fBinary fUnary acc expr : 'r =
  let recurse = fold fValue fBinary fUnary
  match expr with
  | Value value -> fValue acc value

  | Binary (operator, left, right) ->
    let thisAcc = fBinary acc operator left right
    recurse (recurse thisAcc left) right

  | Unary (operator, operand) ->
    let thisAcc = fUnary acc operator operand
    recurse thisAcc operand

let rec foldBack fValue fBinary fUnary expr continuation : 'r =
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

let serialize expr =

  let serializeValue value =
    match value with
    | Boolean true -> "true"
    | Boolean false -> "false"
    | Float f -> sprintf "%g" f
    | Int32 n -> sprintf "%i" n
    | Int64 n -> sprintf "%i" n
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

  foldBack serializeValue serializeBinary serializeUnary expr id

let parseBoolean : ValueParser =
  stringReturn "true" (Boolean true) <|>
  stringReturn "false" (Boolean false) <?>
  "boolean"

let parseNumber : ValueParser =
  let options =
    NumberLiteralOptions.AllowFraction |||
    NumberLiteralOptions.AllowHexadecimal |||
    NumberLiteralOptions.AllowInfinity |||
    NumberLiteralOptions.AllowMinusSign |||
    NumberLiteralOptions.AllowNaN

  let tryNumber wrapper typ str =
    try
      Result.Ok (wrapper (typ str))
    with
      | :? System.OverflowException as e -> Result.Error e

  let orElse fn result =
    match result with
    | Result.Ok x -> Result.Ok x
    | Result.Error _ -> fn ()

  let integer (str : string) =
    tryNumber Int32 int32 str
    |> orElse (fun () -> tryNumber Int64 int64 str)
    |> orElse (fun () -> tryNumber Float float str)

  fun stream ->
    let reply = numberLiteral options "number" stream
    if reply.Status = ReplyStatus.Ok then
      let num = reply.Result
      let result =
        if num.IsNaN then Float nan |> Result.Ok
        elif num.IsInfinity && num.HasMinusSign then Float -infinity |> Result.Ok
        elif num.IsInfinity then Float infinity |> Result.Ok
        elif num.IsInteger then integer num.String
        else tryNumber Float float num.String
      match result with
      | Result.Ok x -> Reply x
      | Result.Error e ->
        stream.Skip -num.String.Length
        Reply (FatalError, messageError e.Message)
    else
      Reply (reply.Status, reply.Error)

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

let parse =

  let parseExpr, parseExprRef = createParserForwardedToRef ()

  let parseValueOrSubExpr : ExprParser =
    choice [
      between (pchar '(') (pchar ')') parseExpr
      parseValue
    ]

  let unaryOp s op p =
    PrefixOperator<Expr, unit, unit> (
      s,
      notFollowedBy letter >>. spaces,
      p, true,
      fun term -> Unary (op, term)
    )

  let binaryOp s op p =
    InfixOperator<Expr, unit, unit> (
      s,
      notFollowedBy letter >>. spaces,
      p, Associativity.Left,
      fun left right -> Binary (op, left, right)
    )

  let opp = OperatorPrecedenceParser<Expr, unit, unit> ()
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

  run parseExpr >> (
    function
    | Success (result, _, _) -> Result.Ok result
    | Failure (message, _, _) -> Result.Error message
  )
