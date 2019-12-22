namespace Rest
open System.Reflection

type RestSuccessDefinition = {
  ResponseType : RestResponse
  Status : RestSuccessStatus
}

module RestSuccessDefinition =

  let applySuccess definition result =
    ((definition ()).Status, result)

  let successResponse definition =
    (definition ()).ResponseType

  let success ct status summary = {
    ResponseType =  {
      ContentType = ct
      Status = RestSuccess status
      Summary = summary
    }
    Status = status
  }

  let postCreated<'A> () =
    success snd Created "{Entity} was created."

  let putCreated<'A> () =
    success snd Created "{Entity} with the specified {Key} was created."

  let deleteOk<'A> () =
    success snd Ok "{Entity} with the specified {Key} was deleted."

  let getOk<'A> () =
    success snd Ok "{Entity} with the specified {Key} was found."

  let putOk<'A> () =
    success snd Ok "{Entity} with the specified {Key} was replaced."

  let queryOk<'A> () =
    success (snd >> typeofSeq) Ok "Returned matching {Entity:plural}."