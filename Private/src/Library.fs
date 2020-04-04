namespace Restfu
open Microsoft.FSharp.Linq.RuntimeHelpers
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open System

[<AutoOpen>]
module Utils =
  let rec extractException<'T when 'T :> exn> (ex : exn) =
    match ex with
    | :? 'T as result -> Some result
    | :? AggregateException as ae ->
      ae.InnerExceptions |> Seq.tryPick extractException<'T>
    | _ -> None

  let ignoreArg0 fn = fun _ arg -> fn arg

  let getterName (expr : Expr<'a -> 'b>) =
    match expr with
    | Lambda (_, FieldGet (_, fi)) -> fi.Name
    | Lambda (_, PropertyGet (_, pi, _)) -> pi.Name
    | _ ->
      invalidArg "expr" "Expected an Expr in the shape of 'fun x -> x.Member'"

  let toLinq (expr : Expr<'a -> 'b>) =
    match expr with
    | Lambda (var, body) ->
      Expr.NewDelegate(typeof<Func<'a, 'b>>, [var], body)
      |> Expr.Cast<Func<'a,'b>>
      |> LeafExpressionConverter.QuotationToLambdaExpression
    | _ -> invalidArg "expr" "Expected a Lambda Expr"

  let getGenericFunctionDef expr =
    match expr with
    | Call (None, mi, _) -> mi.GetGenericMethodDefinition ()
    | _ -> invalidArg "expr" "Expected a generic method call"

  let nullableToOption (nullable : Nullable<'T>) =
    if nullable.HasValue
    then Some nullable.Value
    else None
