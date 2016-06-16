// Copyright © 2016 Massachusetts Institute of Technology
// For license details, refer to the associated LICENSE.md
// file or visit https://opensource.org/licenses/MIT

namespace Mit.Bindle.Scraper

open System
open System.Net

// http://stackoverflow.com/questions/1777221/using-cookiecontainer-with-webclient-class
type CookieAwareWebClient () =
    inherit WebClient()

    member val Cookies : CookieContainer = CookieContainer() with get, set

    override this.GetWebRequest (uri: Uri) : WebRequest =
        let req = base.GetWebRequest(uri)
        match req with
        | :? HttpWebRequest as req' -> req'.CookieContainer <- this.Cookies
        | _ -> ()
        req