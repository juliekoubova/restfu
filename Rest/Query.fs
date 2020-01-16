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