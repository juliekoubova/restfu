namespace Rest

type RestQuery = unit
type RestRequest<'Key, 'Entity> =
| Delete of 'Key
| Get of 'Key
| List of RestQuery
| Post of 'Entity
| Put of 'Key * 'Entity

module RestRequest =
  let map k e request =
    match request with
    | Delete key -> Delete (k key)
    | Get key -> Get (k key)
    | List query -> List query
    | Post entity -> Post (e entity)
    | Put (key, entity) -> Put ((k key), (e entity))

  let mapKey f = map f id
  let mapEntity f = map id f
