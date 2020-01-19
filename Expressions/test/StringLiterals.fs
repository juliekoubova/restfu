module Rest.Tests.StringLiterals
open Expecto
open FParsec
open Rest

let parse (str : string) : Reply<string> =
  use stream = new CharStream<unit> (str, 0, str.Length)
  StringLiteral.parse stream

[<Tests>]
let tests =
  testList "StringLiteralParser" [
    test "single quoted string" {
      Expect.equal
        (parse "'abc'").Result
        "abc"
        "Expected success"
    }
    test "double quoted string" {
      Expect.equal
        (parse "\"aaa\"").Result
        "aaa"
        "Expected success"
    }
    test "escape single quote" {
      Expect.equal
        (parse "'a\\'a'").Result
        "a'a"
        "Expected success"
    }
    test "escape double quote" {
      Expect.equal
        (parse "\"a\\\"a\"").Result
        "a\"a"
        "Expected success"
    }
    test "double quote in a single quoted string" {
      Expect.equal
        (parse "'a\\\"a'").Result
        "a\"a"
        "Expected success"
    }
    test "single quote in a double quoted string" {
      Expect.equal
        (parse "\"a'a\"").Result
        "a'a"
        "Expected success"
    }
    test "unterminated double quote fails" {
      Expect.equal
        (parse "\"aaa").Status
        ReplyStatus.Error
        "Expected error"
    }
  ]