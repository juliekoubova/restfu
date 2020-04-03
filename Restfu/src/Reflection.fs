[<AutoOpen>]
module internal Restfu.Reflection
open System
open System.Reflection

let attributes (attributeProvider : ICustomAttributeProvider) =
  attributeProvider.GetCustomAttributes false
  |> Seq.cast<obj>
  |> Seq.toList

let ofType<'T> =
  Seq.filter (fun a -> box a :? 'T) >> Seq.cast<'T>

let typeofSeq (t : Type) =
  typedefof<seq<_>>.MakeGenericType(t)

let typeofOption (t : Type) =
  typedefof<option<_>>.MakeGenericType(t)