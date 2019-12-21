[<AutoOpen>]
module internal Rest.AspNetCore.Reflection
open System.Reflection

let attributes (attributeProvider : ICustomAttributeProvider) =
  attributeProvider.GetCustomAttributes false
  |> Seq.cast<obj>
  |> Seq.toList

let typeofSeq (t : TypeInfo) =
  typedefof<seq<_>>.MakeGenericType(t).GetTypeInfo()

let typeofOption (t : TypeInfo) =
  typedefof<option<_>>.MakeGenericType(t).GetTypeInfo()