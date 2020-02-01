module Rest.MongoDB.Tests.Main
open Expecto

#nowarn "46" // The identifier 'parallel' is reserved for future use by F#

[<EntryPoint>]
let main argv =
  let cfg = { defaultConfig with parallel = false }
  runTestsInAssembly cfg argv
