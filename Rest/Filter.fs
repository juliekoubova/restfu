namespace Rest
open Microsoft.FSharp.Quotations

type RestFilterValue<'T> =
| Boolean of bool
| Number of decimal
| Property of Expr<'T -> obj>
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

type RestFilterExpr<'T> =
| Value of RestFilterValue<'T>
| Binary of RestFilterBinaryOperator * RestFilterExpr<'T> * RestFilterExpr<'T>
| Unary of RestFilterUnaryOperator * RestFilterExpr<'T>

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

  let serialize filter =
    let serializeValue value =
      match value with
      | Boolean true -> "true"
      | Boolean false -> "false"
      | Number num -> sprintf "%M" num
      | Property expr -> Internal.getterName expr
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