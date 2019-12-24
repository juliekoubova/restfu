namespace Rest.Mapping
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Reflection
open System.Reflection

type Getter = Getter of Expr

type DirectMappingEntry = {
  SourceProperty : PropertyInfo
  TargetProperty : PropertyInfo
}

type TwoWayMappingEntry = {
  SourceProperty : PropertyInfo
  TargetProperty : PropertyInfo
  Convert : Expr
  Revert : Expr
}

type OneWayMappingEntry = {
  SourceProperty : PropertyInfo
  TargetProperty : PropertyInfo
  Convert : Expr
}

type MappingEntry =
| DirectMapping of DirectMappingEntry
| TwoWayMapping of TwoWayMappingEntry
| OneWayMapping of OneWayMappingEntry
| NoMapping of PropertyInfo

module Mapping =
  let sourceProperty var expr =
    match expr with
    | PropertyGet (Some (Var v), property, [])
      when v = var -> Some property
    | _ -> None

  let propertyGetter (varTemplate : Var) pi =
    let var = Var (varTemplate.Name, varTemplate.Type)
    Getter (Expr.Lambda (var, Expr.PropertyGet ((Expr.Var var), pi, [])))

  let sourceGetter varTemplate pi =
    pi |> propertyGetter varTemplate

  let getRecordEntries (expr : Expr) =
    match expr with
    | Lambda (var, body) ->
      match body with
      | NewRecord (targetType, exprs) ->
        let targetExprs =
          FSharpType.GetRecordFields (targetType)
          |> List.ofArray

        exprs
        |> List.map (sourceProperty var)
        |> List.zip targetExprs
        |> List.map (
          function
          | (target, Some source) ->
            DirectMapping { SourceProperty = source; TargetProperty = target }
          | (target, None) -> NoMapping target
        )
      | b -> failwithf "Lambda body is not a NewRecord: %A" b
    | _ -> failwithf "Not a Lambda"