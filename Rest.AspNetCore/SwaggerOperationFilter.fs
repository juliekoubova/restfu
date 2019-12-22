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

  let mergeResponses
    (generateSchema : TypeInfo -> OpenApiSchema)
    (responses : IRestResponseDefinition list)
    =
    let description =
      responses
      |> List.map (fun r -> r.Title)
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
      let props =
        context.ApiDescription.ActionDescriptor.Properties

      match getOperation props with
      | None -> ()
      | Some op ->
        op.Responses
        |> List.groupBy statusCode
        |> List.map (mapSnd (mergeResponses generateSchema))
        |> List.iter (fun (status, response) ->
            operation.Responses.[status] <- response
        )