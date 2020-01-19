namespace Rest.MongoDB
open MongoDB.Driver
open Rest

module private MongoDBResource =
  let create
    (collection : IMongoCollection<'Entity>)
    (entityKey : EntityKey<'Key, 'Entity>)
    =

    let filterByKey key =
      Builders<'Entity>.Filter.Eq(
        entityKey.Expression,
        key
      )

    let delete key =
      collection.DeleteOneAsync(
        filterByKey key
      )
    Crud.create