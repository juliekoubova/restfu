namespace Rest

open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns

type EntityKey<'Key, 'Entity> = {
  Expr : Expr<'Entity -> 'Key>
  Name : string
  Value : 'Entity -> 'Key
}

module EntityKey =
  let getterName (expr : Expr<'a -> 'b>) =
    match expr with
    | Lambda (_, FieldGet (_, fi)) -> fi.Name
    | Lambda (_, PropertyGet (_, pi, _)) -> pi.Name
    | _ ->
      invalidArg "expr" "Expected an Expr in the shape of 'fun x -> x.Member'"

  let validate expr =
    match expr with
    | WithValue (value, _, expr) ->
      let value = value :?> ('Entity -> 'Key)
      let expr = expr :?> Expr<'Entity -> 'Key>
      let name = getterName expr
      { Expr = expr; Name = name; Value = value }
    | _ -> invalidArg "entityKey" "Expected an Expr.WithValue"
