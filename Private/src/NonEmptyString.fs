[<AutoOpen>]
module Restfu.NonEmptyString
open System

type NonEmptyString =
  private NonEmptyString of string
  with member this.Value = match this with NonEmptyString value -> value

module NonEmptyString =
  let create str =
    if String.IsNullOrEmpty str
    then None
    else Some <| NonEmptyString str

