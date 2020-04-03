namespace Restfu
open System.Runtime.CompilerServices

[<assembly: InternalsVisibleTo("Restfu.AspNetCore")>]
[<assembly: InternalsVisibleTo("Restfu.Expressions")>]
[<assembly: InternalsVisibleTo("Restfu.Tests")>]
do ()