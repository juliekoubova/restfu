module Rest.StringLiteral
open FParsec

let parse : Parser<string, unit> =
  let str s = pstring s
  let ch c = pchar c

  let escape =
    anyOf "\"'\\/bfnrt"
    |>> function
        | 'b' -> "\b" | 'f' -> "\u000C" | 'n' -> "\n"
        | 'r' -> "\r" | 't' -> "\t"
        | c -> string c // every other char is mapped to itself

  let unicodeEscape =
    /// converts a hex char ([0-9a-fA-F]) to its integer number (0-15)
    let hex2int c = (int c &&& 15) + (int c >>> 6)*9

    ch 'u' >>. pipe4 hex hex hex hex (fun h3 h2 h1 h0 ->
        (hex2int h3)*4096 + (hex2int h2)*256 + (hex2int h1)*16 + hex2int h0
        |> char |> string
    )

  let escapedCharSnippet = ch '\\' >>. (escape <|> unicodeEscape)
  let normalCharSnippet quote =
    manySatisfy (fun c -> c <> quote && c <> '\\')

  let literal quote =
    between
      (ch quote)
      (ch quote)
      (stringsSepBy (normalCharSnippet quote) escapedCharSnippet)

  literal '"' <|> literal '\'' <?> "string"
