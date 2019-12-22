module internal Rest.Quotations
open Microsoft.FSharp.Quotations

let propertyName (expr: Expr<'a -> 'b>) =
  match expr with
  | Patterns.Lambda (_, body) ->
    match body with
    | Patterns.FieldGet (_, fi) -> fi.Name
    | Patterns.PropertyGet (_, pi, _) -> pi.Name
    | x -> failwithf "Expeceted FieldGet or PropertyGet, got %A" x
  | x -> failwithf "Expected a Lambda, got %A" x