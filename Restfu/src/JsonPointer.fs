namespace Restfu

type JsonPointerToken =
| PropertyName of string
| ArrayIndex of int
| ArrayEnd

type JsonPointer = JsonPointer of JsonPointerToken list

