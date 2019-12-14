namespace Rest

type RestResource<'Id, 'Entity> =
  RestRequest<'Id, 'Entity> -> RestResult<'Id, 'Entity>
