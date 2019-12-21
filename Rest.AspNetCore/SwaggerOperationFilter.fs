namespace Rest.AspNetCore
open Rest
open RestResourceProperties

open Microsoft.OpenApi.Models
open Swashbuckle.AspNetCore.SwaggerGen

type SwaggerOperationFilter() =
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

          OpenApiResponse (Description = description)

        op.Responses
        |> List.groupBy statusCode
        |> List.map (mapSnd mergeResponses)
        |> List.iter (fun (status, response) ->
            operation.Responses.[status] <- response
        )