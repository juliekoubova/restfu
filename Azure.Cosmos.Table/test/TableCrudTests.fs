module Restfu.Azure.Cosmos.Table.TableTests

open Restfu
open Restfu.Tests
open System
open System.Threading
open Expecto

// let mongoUrl =
//   Environment.GetEnvironmentVariable("MONGODB_URL")
//   |> NonEmptyString.create
//   |> Option.orElse (NonEmptyString.create("mongodb://localhost:27017/"))
//   |> Option.get

// let db =
//   MongoClient(mongoUrl.Value).GetDatabase("restfu-test")

// let mutable collectionIndex = 0

// let withMongoDBResource f () =
//   let collectionName =
//     sprintf "thingies-%i" (Interlocked.Increment (&collectionIndex))

//   let collection = db.GetCollection<Thingy>(collectionName)
//   collection.DeleteMany(fun _ -> true) |> ignore

//   let res =
//     MongoDBResource.Create(collection, fun e -> e.Id)
//     |> RestApiExplorer.applyEntityKeyName

//   try
//     f res
//   finally
//     db.DropCollection(collectionName)

[<Tests>]
let tests =
  testList "Azure.Cosmos.Table" [
    testCase "nothing yet" <| fun () -> ()
    // yield! testFixture withMongoDBResource CrudTests.tests
  ]