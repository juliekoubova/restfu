module Restfu.Mapping.Tests.MappingTests
open Restfu.Mapping

open Expecto
open Microsoft.FSharp.Reflection

[<ReflectedDefinition>]
module Domain =
  type Document = {
    Id : int
  }

  type PersonName = {
    FirstName : string
    LastName : string
  }

  type Person = {
    Id : System.Guid
    Age : int
    Name : PersonName
  }

  type DocumentModel = {
    Key : int
  }

  type PersonModel = {
    Id : System.Guid
    Age : int
    FirstName : string
    LastName : string
    FullName : string
  }

  let fullName (name : PersonName) =
    name.FirstName + " " + name.LastName

  let personToModel (person : Person) = {
    Id = person.Id
    Age = person.Age
    FirstName = person.Name.FirstName
    LastName = person.Name.LastName
    FullName = fullName person.Name
  }

  let documentToModel (document : Document) : DocumentModel = {
    Key = document.Id
  }

let personToModel = Mapper.expand Map.empty (<@ Domain.personToModel @>)
let documentToModel = Mapper.expand Map.empty (<@ Domain.documentToModel @>)

let propertyInfo<'T> name =
  FSharpType.GetRecordFields typeof<'T>
  |> Array.tryFind (fun p -> p.Name = name)
  |> Option.get

[<Tests>]
let tests =
  testList "Mapping" [
    testCase "getRecordEntries returns direct mapping" <| (fun _ ->
      let entries = Mapping.getRecordEntries documentToModel
      Expect.equal
        entries
        [
          DirectMapping {
            SourceProperty = propertyInfo<Domain.Document> "Id"
            TargetProperty = propertyInfo<Domain.DocumentModel> "Key"
          }
        ]
        "Unexpected mapping entries"
    )
  ]