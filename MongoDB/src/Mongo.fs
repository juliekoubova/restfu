namespace Rest.MongoDB
open MongoDB.Driver
open MongoDB.Driver.Linq
open Rest
open Rest.RestFailDefinition
open Rest.RestSuccessDefinition
open Microsoft.FSharp.Quotations

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
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
      async {
        let! cancellationToken = Async.CancellationToken

        let! result =
          collection.FindOneAndDeleteAsync(
            (filterByKey key),
            FindOneAndDeleteOptions<_>(),
            cancellationToken
          ) |> Async.AwaitTask

        return
          match box result with
          | null -> DeleteFail (applyFail notFoundKey key key)
          | _ -> DeleteSuccess (applySuccess deleteOk (key, Some result))
      }

    let get key =
      async {
        let! cancellationToken = Async.CancellationToken

        let! result =
          collection
            .Find(filterByKey key)
            .Limit(System.Nullable(1))
            .ToListAsync(cancellationToken)
          |> Async.AwaitTask

        return
          match result.Count with
          | 0 -> GetFail (applyFail notFoundKey key key)
          | _ -> GetSuccess (applySuccess getOk (key, result.[0]))
      }

    let patch (key, patch) =
      let result = PatchFail (applyFail internalServerError () (key, patch))
      async.Return(result)

    let post entity =
      let key = entityKey.Value entity
      async {
        let! cancellationToken = Async.CancellationToken
        try
          let! _ =
            collection.InsertOneAsync(
              entity,
              InsertOneOptions(),
              cancellationToken
            ) |> Async.AwaitTask
          return PostSuccess (applySuccess postCreated (key, Some entity))
        with
          | :? MongoDuplicateKeyException ->
            return PostFail (applyFail alreadyExists key entity)
      }

    let put (key, entity) =
      async {
        let! cancellationToken = Async.CancellationToken
        let! result =
          collection.ReplaceOneAsync(
            filterByKey key,
            entity,
            ReplaceOptions(IsUpsert = true),
            cancellationToken
          ) |> Async.AwaitTask

        return
          match result.UpsertedId with
          | null -> PutSuccess (applySuccess putOk (key, None))
          | _ -> PutSuccess (applySuccess putCreated (key, None))
      }

    let query q =
      async {
        let! cancellationToken = Async.CancellationToken
        let mongoQueryable =
          RestQuery.apply q (collection.AsQueryable()) :?> IMongoQueryable<'Entity>
        let! results =
          mongoQueryable.ToListAsync(cancellationToken) |> Async.AwaitTask
        return QuerySuccess (applySuccess queryOk (q, upcast results))
      }

    Crud.create entityKey delete get patch post put query

[<AbstractClass; Sealed>]
type MongoDBResource private () =

  static member Create
    (
      collection : IMongoCollection<'Entity>,
      [<ReflectedDefinition(true)>] entityKey : Expr<'Entity -> 'Key>
    )
    : RestResource<'Key, 'Entity>
    =
    MongoDBResource.create
      collection
      (entityKey |> EntityKey.validate)