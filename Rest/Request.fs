namespace Rest

type RestRequest<'Key, 'Entity> =
| Delete of 'Key
| Get of 'Key
| Patch of 'Key * JsonPatch
| Post of 'Entity
| Put of 'Key * 'Entity
| Query of RestQuery<'Entity> option

module RestRequest =
  let map k e p q request =
    let qOpt = Option.map q
    match request with
    | Delete key -> Delete (k key)
    | Get key -> Get (k key)
    | Patch (key, patch) -> Patch (k key, p patch)
    | Post entity -> Post (e entity)
    | Put (key, entity) -> Put ((k key), (e entity))
    | Query query -> Query (qOpt query)

  let mapKey f = map f id id id
  let mapEntity f p q = map id f p q
