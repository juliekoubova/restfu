module internal Restfu.AspNetCore.RestResourceProperties
open Restfu

open System.Collections.Generic
open Microsoft.AspNetCore.Mvc.ApplicationModels

let private resourceKey = obj ()
let private operationKey = obj ()

type private Props = IDictionary<obj, obj>

let private tryGet<'T> (key : obj) (props : Props) =
  match props.[key] with
  | :? 'T as value -> Some value
  | _ -> None

let private set<'T, 'M when 'M :> IPropertyModel>
  (key : obj)
  (value : 'T)
  (model : 'M)
  =
  model.Properties.[key] <- value
  model

let setResource<'M when 'M :> IPropertyModel> : IRestResource -> 'M -> 'M =
  set resourceKey

let setOperation<'M when 'M :> IPropertyModel> : RestOperation -> 'M -> 'M =
  set operationKey

let tryGetResource props : IRestResource option =
  tryGet<IRestResource> resourceKey props

let getResource<'K, 'E> props : RestResource<'K, 'E> =
  tryGet<RestResource<'K, 'E>> resourceKey props
  |> Option.orElseWith (fun _ -> failwith "Could not get RestResource")
  |> Option.get

let tryGetOperation props : RestOperation option =
  tryGet operationKey props

