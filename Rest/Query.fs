namespace Rest

type RestOrder =
| Ascending
| Descending

type RestOrderBy = OrderBy of RestExpr * RestOrder


type RestQuery = {
  Filter : RestExpr option
  OrderBy : RestOrderBy list
  Skip : int option
  SkipToken : string option
  Top : int option
}

module RestQuery =

  let empty = {
    Filter = None
    OrderBy = []
    Skip = None
    SkipToken = None
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