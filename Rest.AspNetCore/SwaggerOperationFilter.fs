namespace Rest.AspNetCore
open Rest
open RestResourceProperties

open Microsoft.AspNetCore.Mvc
open Microsoft.OpenApi.Models
open System.Reflection
open Swashbuckle.AspNetCore.SwaggerGen

type SwaggerOperationFilter() =

  [<Literal>]
  let ProblemContentType = "application/problem+json"
  [<Literal>]
  let ResponseContentType = "application/json"

  let mapSnd f (x, y) =
    x, f y

  let statusCode (response : IRestResponseDefinition) =
    response.Status |> StatusCode.code |> int |> string

  let makeResponses (statusResponses : (string * OpenApiResponse) list) =
    let dict = OpenApiResponses ()
    statusResponses |> List.iter (fun (status, response) ->
      dict.[status] <- response
    )
    dict

  let mergeResponses
    (generateSchema : TypeInfo -> OpenApiSchema)
    (entityName : string option)
    (responses : IRestResponseDefinition list)
    =

    let replacements = Map.ofList [
      ("Entity", entityName)
    ]

    let description =
      responses
      |> List.map (fun r -> r.Title |> NaturalLanguage.replaceTokens replacements)
      |> String.concat "\n"

    let getResponseType (r : IRestResponseDefinition) =
      match r.IsSuccess with
      | true -> ResponseContentType, r.ContentType
      | false -> ProblemContentType, typeof<ProblemDetails>.GetTypeInfo ()

    let mediaType schema =
      OpenApiMediaType(Schema = schema)

    let result = OpenApiResponse (Description = description)

    responses
    |> Seq.tryHead
    |> Option.map (
      getResponseType
      >> mapSnd (generateSchema >> mediaType)
    )
    |> Option.iter (fun (k, v) ->
      result.Content.[k] <- v
    )

    result

  interface IOperationFilter with
    member _.Apply (operation, context) =
      let generateSchema t =
        context.SchemaGenerator.GenerateSchema (t, context.SchemaRepository)

      let getSchemaId t =
        generateSchema t |> ignore // ensure schema exists
        let mutable schemaId : string = null
        match context.SchemaRepository.TryGetIdFor (t, &schemaId) with
        | true -> Some schemaId
        | _ -> None

      let props =
        context.ApiDescription.ActionDescriptor.Properties

      let entityName =
        getResourceAnonymous props
        |> Option.bind (fun r -> getSchemaId r.EntityType)

      match getOperation props with
      | None -> ()
      | Some op ->
        operation.Responses <-
          op.Responses
          |> List.groupBy statusCode
          |> List.map (mapSnd (mergeResponses generateSchema entityName))
          |> makeResponses