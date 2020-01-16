namespace Rest
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open System

module internal Internal =
  let ignoreArg0 fn = fun _ arg -> fn arg

  let getterName (expr : Expr<'a -> 'b>) =
    match expr with
    | Lambda (_, FieldGet (_, fi)) -> fi.Name
    | Lambda (_, PropertyGet (_, pi, _)) -> pi.Name
    | _ ->
      invalidArg "expr" "Expected an Expr in the shape of 'fun x -> x.Member'"

  let nullableToOption (nullable : Nullable<'T>) =
    if nullable.HasValue
    then Some nullable.Value
    else None

  let nonEmptyStringToOption (str : string) =
    if String.IsNullOrEmpty str
    then None
    else Some str
