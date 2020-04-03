namespace Restfu
open System.Linq

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

  let private applyFilter filter (queryable : IQueryable<'T>) =
    match filter with
    | None -> queryable
    | Some expr ->
      queryable.Where(RestExprToLinq.convert expr)

  let private applyOrderBy orderBy queryable =
    queryable

  let private applySkip skip (queryable : IQueryable<'T>) =
    match skip with
    | None -> queryable
    | Some skip -> queryable.Skip(skip)

  let private applyTop top (queryable : IQueryable<'T>) =
    match top with
    | None -> queryable
    | Some top -> queryable.Take(top)

  let apply query (queryable : IQueryable<'T>) =
    (applyFilter query.Filter
    >> applyOrderBy query.OrderBy
    >> applySkip query.Skip
    >> applyTop query.Top) queryable