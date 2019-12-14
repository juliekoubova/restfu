namespace Rest

type RestResource<'Key, 'Entity> =
  RestRequest<'Key, 'Entity> -> RestResult<'Key, 'Entity>
