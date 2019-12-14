namespace Rest

type RestRequest<'Id, 'Entity> =
| Delete of 'Id
| Get of 'Id
| List
| Post of 'Entity
| Put of 'Id * 'Entity

module RestRequest =
  let map fId fEntity request =
    match request with
    | Delete id -> Delete (fId id)
    | Get id -> Get (fId id)
    | List -> List
    | Post entity -> Post (fEntity entity)
    | Put (id, entity) -> Put ((fId id), (fEntity entity))

  let mapId f = map f id
  let mapEntity f = map id f
