module Rest.MongoDB.Tests.MongoTests

open MongoDB.Driver
open Rest
open Rest.MongoDB
open Rest.Tests
open Expecto

let client =
  MongoClient(System.Environment.GetEnvironmentVariable("MONGODB_URL"))

let db = client.GetDatabase("restfu-test")

let withMongoDBResource f () =
  let collection = db.GetCollection<Thingy>("thingies")
  collection.DeleteMany(fun _ -> true) |> ignore

  let res =
    MongoDBResource.Create(collection, fun e -> e.Id)

  try
    f res
  finally
    collection.DeleteMany(fun _ -> true) |> ignore

[<Tests>]
let tests =
  testList "MongoDB" [
    yield! testFixture withMongoDBResource CrudTests.tests
  ]