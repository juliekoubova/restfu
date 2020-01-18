namespace Rest.AspNetCore
open Microsoft.OpenApi.Any
open Microsoft.OpenApi.Models
open Swashbuckle.AspNetCore.SwaggerGen
open System

type internal SwaggerRestExprModelFilter () =
  interface ISchemaFilter with
    member _.Apply (schema, context) =
      let isRestExprModel =
        context.Type.IsGenericType &&
        context.Type.GetGenericTypeDefinition() = typedefof<RestExprModel<_>>

      if isRestExprModel then
        schema.Example <- OpenApiString "Some/Property eq 123"
        schema.ExternalDocs <- OpenApiExternalDocs(
          Url = Uri "https://github.com/juliekoubova/tired-rest/wiki/Expressions"
        )
        schema.Title <- "expression"
        schema.Type <- "string"
      else
        ()