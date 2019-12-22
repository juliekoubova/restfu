module internal Rest.AspNetCore.RestResourceProperties
open System.Collections.Generic
open Rest

let private resourceKey = obj()
let private operationKey = obj()

type private Props = IDictionary<obj, obj>

let private get<'T> key (properties : Props) =
  match properties.[key] with
  | :? 'T as value -> Some value
  | _ -> None

let private set key (properties : Props) value =
  properties.[key] <- value

let setResource : Props -> IRestResource -> unit =
  set resourceKey

let setOperation : Props -> RestOperation -> unit =
  set operationKey

let getResourceAnonymous props : IRestResource option =
  get resourceKey props

let getResource<'K, 'E> props : RestResource<'K, 'E> option =
  get resourceKey props

let getOperation props : RestOperation option =
  get operationKey props

