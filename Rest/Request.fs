namespace Rest

type RestQuery<'Entity> =
| RestQuery of unit

type RestRequest<'Key, 'Entity> =
| Delete of 'Key
| Get of 'Key
| Post of 'Entity
| Put of 'Key * 'Entity
| Query of RestQuery<'Entity>

module RestRequest =
  let map k e q request =
    match request with
    | Delete key -> Delete (k key)
    | Get key -> Get (k key)
    | Post entity -> Post (e entity)
    | Put (key, entity) -> Put ((k key), (e entity))
    | Query query -> Query (q query)

  let mapKey f = map f id id
  let mapEntity f q = map id f q
