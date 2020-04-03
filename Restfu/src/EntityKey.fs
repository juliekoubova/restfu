namespace Restfu

open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open System
open System.Linq.Expressions

type EntityKey<'Key, 'Entity> = {
  Expr : Expr<'Entity -> 'Key>
  Expression : Expression<Func<'Entity, 'Key>>
  Name : string
  Value : 'Entity -> 'Key
}

module EntityKey =

  let validate expr =
    match expr with
    | WithValue (value, _, expr) ->
      let value = value :?> ('Entity -> 'Key)
      let expr = expr :?> Expr<'Entity -> 'Key>
      let name = getterName expr
      let linq = toLinq expr
      { Expr = expr; Expression = linq; Name = name; Value = value }
    | _ -> invalidArg "entityKey" "Expected an Expr.WithValue"
