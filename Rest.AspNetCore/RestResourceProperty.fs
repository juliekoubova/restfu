module internal Rest.AspNetCore.RestResourceProperty
open System.Collections.Generic
open Rest

let private resourceKey = obj()

let setResource
  (properties : IDictionary<obj, obj>)
  (resource : IRestResource)
  =
  properties.[resourceKey] <- resource

let getResource
  (properties : IDictionary<obj, obj>)
  =
  properties.[resourceKey] :?> IRestResource
