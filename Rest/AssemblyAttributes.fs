namespace Rest
open System.Runtime.CompilerServices

[<assembly: InternalsVisibleTo("Rest.AspNetCore")>]
[<assembly: InternalsVisibleTo("Rest.Expressions")>]
[<assembly: InternalsVisibleTo("Rest.Tests")>]
do ()