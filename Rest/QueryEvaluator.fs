module Rest.QueryEvaluator
open Microsoft.FSharp.Quotations
open System
open Microsoft.FSharp.Linq.RuntimeHelpers
open System.Reflection

let applyFilter<'T> ({ Filter = filter } : RestQuery<'T>) (seq : 'T seq) =
  match filter with
  | None -> seq
  | Some expr ->

    let var = Var ("entity", typeof<'T>)

    let compileLiteral obj =
      Expr.Value (obj, obj.GetType ())

    let compileProperty (properties : PropertyInfo list) =
      let getProperty (pi : PropertyInfo) prev =
        Expr.PropertyGet (prev, pi)
      List.foldBack getProperty properties (Expr.Var var)

    let compileConvert (target : Type) expr =
      let convert = <@@ System.Convert.ChangeType (%%expr, target) @@>
      Expr.Coerce (convert, target)

    let compileBinary op left right =
      match op with
      | ExprAst.Equal -> <@@ %%left = %%right @@>
      | ExprAst.NotEqual -> <@@ %%left <> %%right @@>
      | ExprAst.LessThan -> <@@ %%left < %%right @@>
      | ExprAst.LessThanOrEqual -> <@@ %%left <= %%right @@>
      | ExprAst.GreaterThan -> <@@ %%left > %%right @@>
      | ExprAst.GreaterThanOrEqual -> <@@ %%left >= %%right @@>
      | ExprAst.And -> <@@ %%left && %%right @@>
      | ExprAst.Or -> <@@ %%left || %%right @@>

    let compileUnary op expr =
      match op with
      | ExprAst.Not -> <@@ not %%expr @@>

    let body =
      RestExpr.foldBack
        compileLiteral
        compileProperty
        compileConvert
        compileBinary
        compileUnary
        expr
        id

    let lambda =
      Expr.Lambda (var, body)

    let linq =
      lambda
      :?> Expr<'T -> bool>
      |> LeafExpressionConverter.QuotationToLambdaExpression

    let compiled = linq.Compile ()

    seq |> Seq.filter compiled