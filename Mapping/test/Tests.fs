module Rest.Mapping.Tests

open FsUnit
open Xunit
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

[<Fact>]
let ``getRecordEntries returns direct mapping`` () =
  let entries = Mapping.getRecordEntries documentToModel
  entries |> should equal [
    DirectMapping {
      SourceProperty = propertyInfo<Domain.Document> "Id"
      TargetProperty = propertyInfo<Domain.DocumentModel> "Key"
    }
  ]
  // Mapper.Create Domain.personToModel
  // Mapper.Create (fun x -> x * 2)