module Rest.MongoDB.Tests.MongoTests

open MongoDB.Driver
open Rest
open Rest.MongoDB
open Rest.Tests
open Expecto

let client =
  MongoClient(System.Environment.GetEnvironmentVariable("MONGODB_URL"))

let db = client.GetDatabase("restfu-test")
let mutable collectionIndex = 0

let withMongoDBResource f () =
  let collectionName =
    sprintf "thingies-%i" (System.Threading.Interlocked.Increment (&collectionIndex))

  let collection = db.GetCollection<Thingy>(collectionName)
  collection.DeleteMany(fun _ -> true) |> ignore

  let res =
    MongoDBResource.Create(collection, fun e -> e.Id)
    |> RestApiExplorer.applyEntityKeyName

  try
    f res
  finally
    db.DropCollection(collectionName)

[<Tests>]
let tests =
  testList "MongoDB" [
    yield! testFixture withMongoDBResource CrudTests.tests
  ]