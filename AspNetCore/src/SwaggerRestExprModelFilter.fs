namespace Restfu.AspNetCore
open Microsoft.OpenApi.Any
open Microsoft.OpenApi.Models
open Swashbuckle.AspNetCore.SwaggerGen
open System

type internal SwaggerRestExprModelFilter () =

  let removeProperties (repository : SchemaRepository) (schema : OpenApiSchema) =
    schema.Properties.Values |> Seq.iter (fun prop ->
      prop.AllOf
      |> Seq.filter (fun schema -> schema.Reference.IsLocal)
      |> Seq.iter (fun schema ->
        repository.Schemas.Remove schema.Reference.Id |> ignore
      )
    )
    schema.Properties.Clear() |> ignore

  let exprModelTitle (repository : SchemaRepository) (modelType : Type) =
    let mutable id = null
    if repository.TryGetIdFor(modelType, &id) then
      let title = repository.Schemas.[id].Title
      if isNull title
      then Some id
      else Some title
    else
      None

  interface ISchemaFilter with
    member _.Apply(schema, context) =
      if RestExprModel.isRestExprModel context.Type then

        let modelType = RestExprModel.modelType context.Type
        let modelTitle =
          exprModelTitle context.SchemaRepository modelType

        removeProperties context.SchemaRepository schema

        schema.Description <-
          modelTitle
          |> Option.map (sprintf "Expression referencing properties of %s")
          |> Option.orElse (Some "Expression")
          |> Option.get

        schema.Example <- OpenApiString "Some/Property eq 123"
        schema.ExternalDocs <- OpenApiExternalDocs(
          Url = Uri "https://github.com/juliekoubova/tired-rest/wiki/Expressions"
        )
        schema.Type <- "string"
      else
        ()