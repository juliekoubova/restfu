namespace Restfu.AspNetCore
open Microsoft.Extensions.Options
open Swashbuckle.AspNetCore.SwaggerGen

type internal SwaggerSchemaIdSelector () =
  interface IPostConfigureOptions<SchemaGeneratorOptions> with
    member _.PostConfigure(_, options) =
      let prev = options.SchemaIdSelector.Invoke
      options.SchemaIdSelector <-
        (fun typ ->
          if RestExprModel.isRestExprModel typ then
            prev (typ.GetGenericArguments().[0]) |> sprintf "Expression(%s)"
          else
            prev typ
        )