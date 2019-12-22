[<AutoOpen>]
module internal Rest.Reflection
open System.Reflection

let attributes (attributeProvider : ICustomAttributeProvider) =
  attributeProvider.GetCustomAttributes false
  |> Seq.cast<obj>
  |> Seq.toList

let ofType<'T> =
  Seq.filter (fun a -> box a :? 'T) >> Seq.cast<'T>

let typeofSeq (t : TypeInfo) =
  typedefof<seq<_>>.MakeGenericType(t).GetTypeInfo()

let typeofOption (t : TypeInfo) =
  typedefof<option<_>>.MakeGenericType(t).GetTypeInfo()