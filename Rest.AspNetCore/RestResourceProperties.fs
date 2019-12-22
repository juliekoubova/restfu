module internal Rest.AspNetCore.RestResourceProperties
open Rest

open System.Collections.Generic
open Microsoft.AspNetCore.Mvc.ApplicationModels

let private resourceKey = obj ()
let private operationKey = obj ()

type private Props = IDictionary<obj, obj>

let private get<'T> (key : obj) (props : Props) =
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

let getResourceAnonymous props : IRestResource option =
  get<IRestResource> resourceKey props

let getResource<'K, 'E> props : RestResource<'K, 'E> option =
  get<RestResource<'K, 'E>> resourceKey props

let getOperation props : RestOperation option =
  get operationKey props

