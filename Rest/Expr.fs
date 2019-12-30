namespace Rest
open System
open System.Reflection

type RestExpr<'T> =
| Convert of Type * RestExpr<'T>
| Literal of obj
| Property of PropertyInfo list
| Unary of ExprAst.UnaryOperator * RestExpr<'T>
| Binary of ExprAst.BinaryOperator * RestExpr<'T> * RestExpr<'T>

module RestExpr =
  let exprType =
    function
    | Convert (target, _) -> target
    | Binary _ -> typeof<bool>
    | Unary _ -> typeof<bool>
    | Literal value -> value.GetType ()
    | Property list -> (List.last list).PropertyType

  let getProperty (typ : Type) name =
    match typ.GetProperty name with
    | null ->
      Error (sprintf "Property %s doesn't exist on type %s" name typ.Name)
    | pi -> Ok pi

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

  let tryCoerce (l, r) typ =
    let tl = exprType l
    let tr = exprType r

    if tl = typ && tr <> typ then
      l, Convert (typ, r)
    elif tl <> typ && tr = typ then
      Convert (typ, l), r
    else
      l, r

  let coerceTypes (l, r) =
    List.fold tryCoerce (l, r) [
      typeof<float>
      typeof<float32>
      typeof<decimal>
      typeof<int64>
      typeof<int32>
      typeof<int64>
    ]

  let validate entityType ast =

    let validateValue =
      let literal x = x |> box |> Literal |> Ok
      function
      | ExprAst.Boolean b -> literal b
      | ExprAst.Number num -> literal num
      | ExprAst.String str -> literal str
      | ExprAst.Property path ->
        validatePropertyPath entityType path
        |> Result.map Property

    let expectType (expected : Type) (actual : Type) result =
      if expected = actual then
        Ok result
      else
        Error (sprintf
          "%s : This expression was expected to have type '%s' but here has type '%s'"
          "TODO"
          expected.Name
          actual.Name
        )

    let validateBinary op l r =
      l |> Result.bind (fun l ->
        r |> Result.bind (fun r ->
          let l, r = coerceTypes (l, r)
          let lType = exprType l
          let rType = exprType r

          let relational op =
            expectType lType rType (Binary (op, l, r))

          let logical op =
            expectType typeof<bool> lType ()
            |> Result.map (fun _ -> expectType typeof<bool> rType ())
            |> Result.map (fun _ -> Binary (op, l, r))

          match op with
          | ExprAst.Equal -> relational ExprAst.Equal
          | ExprAst.NotEqual -> relational ExprAst.NotEqual
          | ExprAst.GreaterThan -> relational ExprAst.GreaterThan
          | ExprAst.GreaterThanOrEqual -> relational ExprAst.GreaterThanOrEqual
          | ExprAst.LessThan -> relational ExprAst.LessThan
          | ExprAst.LessThanOrEqual -> relational ExprAst.LessThanOrEqual
          | ExprAst.And -> logical ExprAst.And
          | ExprAst.Or -> logical ExprAst.Or
        )
       )

    let validateUnary op expr =
      expr |> Result.bind (fun expr ->
        match op with
        | ExprAst.Not ->
          expectType typeof<bool> (exprType expr) (Unary (ExprAst.Not, expr))
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
