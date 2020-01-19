module Rest.RestExprToLinq
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Linq.RuntimeHelpers
open System
open System.Reflection
open Rest

let private equality =
  getGenericFunctionDef <@ LanguagePrimitives.GenericEquality 0 0 @>

let private comparison =
  getGenericFunctionDef <@ LanguagePrimitives.GenericComparison 0 0 @>

let convert (expr : RestExpr<'T>) =
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

  let compileBinary op (left : Expr) (right : Expr) =

    let equals =
      Expr.Call (equality.MakeGenericMethod([|left.Type|]), [left; right])

    let compare =
      Expr.Call (comparison.MakeGenericMethod([| left.Type|]), [left;right])

    match op with
    | ExprAst.Equal -> equals
    | ExprAst.NotEqual -> <@@ not %%equals @@>
    | ExprAst.LessThan -> <@@ %%compare < 0 @@>
    | ExprAst.LessThanOrEqual -> <@@ %%compare <= 0 @@>
    | ExprAst.GreaterThan -> <@@ %%compare > 0 @@>
    | ExprAst.GreaterThanOrEqual -> <@@ %%compare >= 0 @@>
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
    Expr.NewDelegate (typeof<Func<'T, bool>>, [var], body)

  Expr.Cast<Func<'T, bool>> lambda
  |> LeafExpressionConverter.QuotationToLambdaExpression

let compile expr =
  let linq = convert expr
  let compiled = linq.Compile()
  compiled
