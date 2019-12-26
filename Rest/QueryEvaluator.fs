module Rest.QueryEvaluator
open Microsoft.FSharp.Quotations
open System
open Microsoft.FSharp.Linq.RuntimeHelpers

// let private applyFilter<'T> { Filter = filter } seq =
//   match filter with
//   | None -> seq
//   | Some f ->

//     let rec filterExpr var (currentType : Type) e =
//       match e with
//       | Property (Getter getter) ->
//         <@@ (%getter) (%var) @@>

//       | Number num ->
//         let o = box num
//         <@@ System.Convert.ChangeType (o, currentType) @@>

//       | Boolean b ->
//         <@@ b @@>

//       | Equal (x, y) ->
//         let xe = filterExpr var currentType x
//         let ye = filterExpr var currentType y
//         <@@ (%%xe) = (%%ye) @@>

//       | _ -> failwith "todo"

//     let lambdaVar = Var ("x", typeof<'T>)
//     let lambda =
//       Expr.Lambda (
//         lambdaVar,
//         (filterExpr (Expr.Var (lambdaVar) typeof<obj>))
//       )

//     let linqExpr =
//       lambda
//       |> LeafExpressionConverter.QuotationToLambdaExpression
//       |> unbox<Expression

//     seq |> Seq.filter linqExpr

