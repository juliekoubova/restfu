namespace Restfu.Mapping
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.DerivedPatterns
open Microsoft.FSharp.Quotations.ExprShape
open Microsoft.FSharp.Quotations.Patterns

module Mapper =
  /// The parameter 'vars' is an immutable map that assigns expressions to variables
  /// (as we recursively process the tree, we replace all known variables)
  let rec expand vars expr =

    // First recursively process & replace variables
    let expanded =
      match expr with
      // If the variable has an assignment, then replace it with the expression
      | ShapeVar v when Map.containsKey v vars -> vars.[v]
      // Apply 'expand' recursively on all sub-expressions
      | ShapeVar v -> Expr.Var v
      | Call (body, MethodWithReflectedDefinition meth, args) ->
          let this = match body with Some b -> Expr.Application(meth, b) | _ -> meth
          let res = Expr.Applications(this, [ for a in args -> [a]])
          expand vars res
      | ShapeLambda (v, expr) ->
          Expr.Lambda (v, expand vars expr)
      | ShapeCombination (o, exprs) ->
          RebuildShapeCombination (o, List.map (expand vars) exprs)

    // After expanding, try reducing the expression - we can replace 'let'
    // expressions and applications where the first argument is lambda
    match expanded with
    | Application (ShapeLambda (v, body), assign)
    | Let (v, assign, body) ->
        expand (Map.add v (expand vars assign) vars) body
    | _ -> expanded

[<AbstractClass; Sealed>]
type Mapper private () =
  static member Create
    ([<ReflectedDefinition(true)>] convert : Expr<'X -> 'Y>)
    =
    printfn "%A" (Mapper.expand Map.empty convert)

