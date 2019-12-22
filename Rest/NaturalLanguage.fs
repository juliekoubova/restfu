module Rest.NaturalLanguage
open Pluralize.NET
open System.Text.RegularExpressions

let private pluralizer = Pluralizer()

let private token = Regex ("""(?x)
  \{
  (?<Token> [A-Za-z]+)
  (?:
    :
    (?: (?<Plural> plural) | (?<Possessive> possessive) )
  )?
  \}
  """)

let private possessivize (noun : string) =
  if (noun.EndsWith "s") then
    noun + "'"
  else
    noun + "'s"

let replaceTokens
  (replacements : Map<string, string option>)
  (template : string)
  =
  token.Replace (template, fun m ->
    let token = m.Groups.["Token"].Value
    let plural = m.Groups.["Plural"].Success
    let possessive = m.Groups.["Possessive"].Success

    let replacement =
      match Map.tryFind token replacements with
      | Some (Some replacement) -> replacement
      | _ -> token

    match (plural, possessive) with
    | (true, _) -> pluralizer.Pluralize(replacement)
    | (_, true) -> possessivize replacement
    | _ ->  replacement
  )
