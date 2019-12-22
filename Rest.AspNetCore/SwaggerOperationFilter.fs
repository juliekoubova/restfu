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


  interface IOperationFilter with
    member _.Apply (operation, context) =
      let op = getOperation context.ApiDescription.ActionDescriptor.Properties
      match op with
      | None -> ()
      | Some op ->

        let mapSnd f (x, y) =
          x, f y

        let statusCode (response : IRestResponseDefinition) =
          response.Status |> StatusCode.code |> int |> string

        let mergeResponses (responses : IRestResponseDefinition list) =
          let description =
            responses
            |> List.map (fun r -> r.Title)
            |> String.concat "\n"

          let getResponseType (r : IRestResponseDefinition) =
            match r.IsSuccess with
            | true -> ResponseContentType, r.ContentType
            | false -> ProblemContentType, typeof<ProblemDetails>.GetTypeInfo ()

          let generateSchema t =
            context.SchemaGenerator.GenerateSchema (t, context.SchemaRepository)

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

        op.Responses
        |> List.groupBy statusCode
        |> List.map (mapSnd mergeResponses)
        |> List.iter (fun (status, response) ->
            operation.Responses.[status] <- response
        )