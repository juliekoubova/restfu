namespace Restfu.AspNetCore
open Restfu
open System
open System.ComponentModel

[<AllowNullLiteral>]
[<TypeDescriptionProvider(typeof<RestExprModelTypeDescriptionProvider>)>]
type RestExprModel<'T> (expr : RestExpr<'T>) =
  member _.Expression with get () = expr

and internal RestExprModelTypeDescriptionProvider () =
  inherit TypeDescriptionProvider ()
  override _.GetTypeDescriptor (objectType : Type, _instance : obj) =
    upcast (RestExprModelTypeDescriptor objectType)

and internal RestExprModelTypeDescriptor (targetType : Type) =
  inherit CustomTypeDescriptor  ()
  override _.GetConverter () =
    let entityType = (targetType.GetGenericArguments ()).[0]
    let converterType =
      typedefof<RestExprModelTypeConverter<_>>.MakeGenericType entityType
    Activator.CreateInstance converterType :?> TypeConverter

and internal RestExprModelTypeConverter<'T> () =
  inherit TypeConverter ()
  override _.CanConvertFrom (_context, sourceType) =
    sourceType = typeof<string>

  override _.ConvertFrom (_context, _culture, source) =
    match RestExpr.parse<'T> (source :?> string) with
    | Result.Ok expr -> upcast (RestExprModel expr)
    | Error error -> raise (FormatException error)

module RestExprModel =
  let isRestExprModel (typ : Type) =
    typ.IsGenericType &&
    typ.GetGenericTypeDefinition() = typedefof<RestExprModel<_>>

  let modelType (typ: Type) =
    typ.GetGenericArguments().[0]
