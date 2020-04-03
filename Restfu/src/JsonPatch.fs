namespace Restfu
open System.Text.Json

type JsonPatchOp =
| Add of JsonPointer * JsonElement
| Remove of JsonPointer
| Replace of JsonPointer * JsonElement
| Test of JsonPointer * JsonElement

type JsonPatch = JsonPatch of JsonPatchOp list