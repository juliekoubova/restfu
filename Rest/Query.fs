namespace Rest

type RestOrder =
| Ascending
| Descending

type RestOrderBy<'T> = OrderBy of RestExpr<'T> * RestOrder

type RestQuery<'T> = {
  Filter : RestExpr<'T> option
  OrderBy : RestOrderBy<'T> list
  Skip : int option
  Top : int option
}

module RestQuery =

  let empty = {
    Filter = None
    OrderBy = []
    Skip = None
    Top = None
  }

  let private applyFilter filter seq =
    match filter with
    | None -> seq
    | Some expr ->
      let compiled = RestExprToLinq.compile expr
      Seq.filter compiled.Invoke seq

  let private applyOrderBy orderBy seq =
    seq

  let private applySkip skip seq =
    match skip with
    | None -> seq
    | Some skip -> Seq.skip skip seq

  let private applyTop top seq =
    match top with
    | None -> seq
    | Some top -> Seq.take top seq

  let apply query seq =
    (applyFilter query.Filter
    >> applyOrderBy query.OrderBy
    >> applySkip query.Skip
    >> applyTop query.Top) seq