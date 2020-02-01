module Rest.MongoDB.Tests.Main
open Expecto

[<EntryPoint>]
let main argv =
  let cfg = { defaultConfig with ``parallel`` = false }
  runTestsInAssembly cfg argv
