namespace Rest.AspNetCore
open Swashbuckle.AspNetCore.SwaggerGen

type SwaggerReserveSchemaIds
  (
      registrations : IRestApiRegistration seq
  ) =
  interface IOperationFilter with
    member _.Apply (_, context) =
      registrations
      |> Seq.map (fun reg -> reg.Resource.EntityType, reg.Resource.EntityName)
      |> Seq.iter context.SchemaRepository.ReserveIdFor