namespace Rest.AspNetCore
open Rest
open RestResourceProperties

open Microsoft.AspNetCore.Mvc
open Microsoft.OpenApi.Models
open System.Reflection
open Swashbuckle.AspNetCore.SwaggerGen

type SwaggerOperationFilter() =

  [<Literal>]
  let ProblemMediaType = "application/problem+json"
  [<Literal>]
  let ResponseMediaType = "application/json"

  let mapSnd f (x, y) =
    x, f y

  let statusCode (response : RestResponse) =
    response.Status |> StatusCode.code |> int |> string

  let makeResponses (statusResponses : (string * OpenApiResponse) list) =
    let dict = OpenApiResponses ()
    statusResponses |> List.iter (fun (status, response) ->
      dict.[status] <- response
    )
    dict

  let mergeResponses
    (generateSchema : TypeInfo -> OpenApiSchema)
    (resource : IRestResource)
    (responses : RestResponse list)
    =

    let description =
      responses
      |> List.map (fun r -> r.Summary)
      |> String.concat "\n"

    let getResponseType (r : RestResponse) =
      match r.Status with
      | RestSuccess _ ->
        ResponseMediaType, (r.ContentType (resource.KeyType, resource.EntityType))
      | RestFail _ ->
        ProblemMediaType, typeof<ProblemDetails>.GetTypeInfo ()

    let mediaType schema =
      OpenApiMediaType(Schema = schema)

    let result = OpenApiResponse (Description = description)

    responses
    |> Seq.tryExactlyOne
    |> Option.map (getResponseType >> mapSnd (generateSchema >> mediaType))
    |> Option.iter (fun (mediaTypeName, mediaType) ->
      result.Content.[mediaTypeName] <- mediaType
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

      match (tryGetResource props, tryGetOperation props) with
      | Some res, Some op ->
        operation.Summary <- Option.toObj op.Summary
        operation.Responses <-
          op.Responses
          |> List.groupBy statusCode
          |> List.map (mapSnd (mergeResponses generateSchema res))
          |> makeResponses
      | _ -> ()