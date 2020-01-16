namespace Rest
open System
open System.Reflection

type RestExpr =
| Convert of Type * RestExpr
| Literal of obj
| Property of PropertyInfo list
| Unary of ExprAst.UnaryOperator * RestExpr
| Binary of ExprAst.BinaryOperator * RestExpr * RestExpr

module RestExpr =
  let exprType =
    function
    | Convert (target, _) -> target
    | Binary _ -> typeof<bool>
    | Unary (ExprAst.Not, _) -> typeof<bool>
    | Literal value -> value.GetType ()
    | Property list -> (List.last list).PropertyType

  let getProperty (typ : Type) name =
    match typ.GetProperty name with
    | null ->
      Error (sprintf "Property %s doesn't exist on type %s" name typ.Name)
    | pi -> Ok pi

  let rec foldBack
    fLiteral
    fProperty
    fConvert
    fBinary
    fUnary
    expr
    continuation
    =
    let recurse = foldBack fLiteral fProperty fConvert fBinary fUnary
    match expr with
    | Literal value -> continuation (fLiteral value)
    | Property path -> continuation (fProperty path)
    | Convert (typ, expr) ->
      recurse expr (fun expr -> continuation (fConvert typ expr))
    | Unary (op, expr) ->
      recurse expr (fun expr -> continuation (fUnary op expr))
    | Binary (op, left, right) ->
      recurse left (fun left ->
        recurse right (fun right ->
          continuation (fBinary op left right)
        )
      )

  let serialize expr =

    let serializeLiteral (value : obj) =
      match value with
      | :? bool as b -> sprintf "%b" b
      | :? Int16 as i -> sprintf "%i" i
      | :? Int32 as i -> sprintf "%i" i
      | :? Int64 as i -> sprintf "%i" i
      | :? float as f -> sprintf "%g" f
      | :? float32 as f -> sprintf "%g" f
      | :? string as s -> sprintf "'%s'" s // TODO escape
      | other -> failwithf "Unable to serialize literal %A" other

    let serializeConvert (typ : Type) expr =
      sprintf "cast(%s, %s)" expr typ.Name

    let serializeProperty (path : PropertyInfo list) =
      path |> List.map (fun p -> p.Name) |> String.concat "/"

    let serializeUnary operator expr =
      let operator =
        match operator with
        | ExprAst.Not -> "not"
      sprintf "(%s %s)" operator expr

    let serializeBinary operator left right =
      let operator =
        match operator with
        | ExprAst.Equal -> "eq"
        | ExprAst.NotEqual -> "ne"
        | ExprAst.LessThan -> "lt"
        | ExprAst.LessThanOrEqual -> "le"
        | ExprAst.GreaterThan -> "gt"
        | ExprAst.GreaterThanOrEqual -> "ge"
        | ExprAst.And -> "and"
        | ExprAst.Or -> "or"

      sprintf "(%s %s %s)" left operator right

    foldBack
      serializeLiteral
      serializeProperty
      serializeConvert
      serializeBinary
      serializeUnary
      expr
      id

  let validateProperty result name =
    result |> Result.bind (fun (typ, list) ->
      getProperty typ name |> Result.map (
        fun pi -> pi.PropertyType, pi :: list
      )
    )

  let validatePropertyPath (typ : Type) path =
    List.fold
      validateProperty
      (Ok (typ, []))
      path
    |> Result.map (snd >> List.rev)

  let coercePriority = [
      typeof<float>
      typeof<float32>
      typeof<decimal>
      typeof<int64>
      typeof<int32>
      typeof<int64>
    ]

  let canCoerce from target =
    List.contains from coercePriority &&
    List.contains target coercePriority

  let convert expr (typ : Type) =
    match expr with
    | Literal value -> Literal (System.Convert.ChangeType (value, typ))
    | expr -> Convert (typ, expr)

  let tryCoerce (l, r) typ =
    let tl = exprType l
    let tr = exprType r

    if tl = typ && tr <> typ then
      l, convert r typ
    elif tl <> typ && tr = typ then
      convert l typ, r
    else
      l, r

  let coerceTypes (l, r) =
    let tl = exprType l
    let tr = exprType r

    if tl <> tr && canCoerce tl tr then
      List.fold tryCoerce (l, r) coercePriority
    else
      (l, r)

  let validate entityType ast : Result<RestExpr, string> =

    let validateValue =
      let literal x = x |> box |> Literal |> Ok
      function
      | ExprAst.Boolean b -> literal b
      | ExprAst.Float f -> literal f
      | ExprAst.Int32 n -> literal n
      | ExprAst.Int64 n -> literal n
      | ExprAst.String str -> literal str
      | ExprAst.Property path ->
        validatePropertyPath entityType path
        |> Result.map Property

    let expectType (expected : Type) expr result =
      let actual = exprType expr
      if expected = actual then
        Ok result
      else
        Error (sprintf
          "%s : This expression was expected to have type '%s' but here has type '%s'"
          (serialize expr)
          expected.Name
          actual.Name
        )

    let validateBinary op l r =
      l |> Result.bind (fun l ->
        r |> Result.bind (fun r ->
          let l, r = coerceTypes (l, r)
          let lType = exprType l

          let relational op =
            expectType lType r (Binary (op, l, r))

          let logical op =
            expectType typeof<bool> l ()
            |> Result.map (fun _ -> expectType typeof<bool> r ())
            |> Result.map (fun _ -> Binary (op, l, r))

          match op with
          | ExprAst.Equal
          | ExprAst.NotEqual
          | ExprAst.GreaterThan
          | ExprAst.GreaterThanOrEqual
          | ExprAst.LessThan
          | ExprAst.LessThanOrEqual
            -> relational op
          | ExprAst.And
          | ExprAst.Or
            -> logical op
        )
       )

    let validateUnary op expr =
      expr |> Result.bind (fun expr ->
        match op with
        | ExprAst.Not ->
          expectType typeof<bool> expr (Unary (ExprAst.Not, expr))
      )

    ExprAst.foldBack
      validateValue
      validateBinary
      validateUnary
      ast
      id

  let parse entityType str =
    ExprAst.parse str
    |> Result.bind (validate entityType)
