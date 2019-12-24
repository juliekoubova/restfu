namespace Rest

type RestOrder =
| Ascending
| Descending

type RestOrderBy = OrderBy of string * RestOrder

type RestFilterExpr =
| Property of JsonPointer
| Number of decimal
| Boolean of bool

| Equal of RestFilterExpr * RestFilterExpr
| NotEqual of RestFilterExpr * RestFilterExpr
| GreaterThan of RestFilterExpr * RestFilterExpr
| GreaterThanOrEqual of RestFilterExpr * RestFilterExpr
| LessThan of RestFilterExpr * RestFilterExpr
| LessThanOrEqual of RestFilterExpr * RestFilterExpr
| And of RestFilterExpr * RestFilterExpr
| Or of RestFilterExpr * RestFilterExpr
| Not of RestFilterExpr

type RestQuery = {
  Filter : RestFilterExpr option
  OrderBy : RestOrderBy list
  Skip : int option
  Top : int option
}

