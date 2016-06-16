// Copyright © 2016 Massachusetts Institute of Technology
// For license details, refer to the associated LICENSE.md
// file or visit https://opensource.org/licenses/MIT

namespace Mit.Bindle.Server

type CaseInsensitiveStringSet (contents: seq<string>) =
    let set = System.Collections.Generic.HashSet<string>(contents, System.StringComparer.CurrentCultureIgnoreCase)
    
    member this.Count : int = set.Count

    member this.Contains (needle: string) : bool = set.Contains needle