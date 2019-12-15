namespace Rest

module internal Internal =
  let ignoreArg0 fn = fun _ arg -> fn arg