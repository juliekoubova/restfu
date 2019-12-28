namespace Rest
open Microsoft.FSharp.Quotations

type RestOrder =
| Ascending
| Descending

type RestQueryProperty<'T> = RestQueryProperty of Expr<'T -> obj>
type RestOrderBy<'T> = OrderBy of RestQueryProperty<'T> * RestOrder


type RestQuery<'T> = {
  Filter : RestFilterExpr option
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


  let private applyFilter { Filter = filter } seq =
    seq

  let private applyOrderBy query seq =
    seq

  let private applySkip query seq =
    seq

  let private applyTop query seq =
    seq

  let apply query seq =
    (applyFilter
    >> applyOrderBy
    >> applySkip
    >> applyTop) query seq