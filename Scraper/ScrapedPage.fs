// Copyright © 2016 Massachusetts Institute of Technology
// For license details, refer to the associated LICENSE.md
// file or visit https://opensource.org/licenses/MIT

namespace Mit.Bindle.Scraper

open System
open System.Net

type ScrapedPage = {
    CsvUris: Uri list
    LastConnectionTime: DateTimeOffset option
    NextConnectionTime: DateTimeOffset option
    Uri: Uri
    Cookies: CookieContainer
    }
    with
        static member create uri csvUris lastConnectionTime nextConnectionTime cookies =
            {
                CsvUris = csvUris
                LastConnectionTime = lastConnectionTime
                NextConnectionTime = nextConnectionTime
                Uri = uri
                Cookies = cookies
            }