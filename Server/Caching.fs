// Copyright © 2016 Massachusetts Institute of Technology
// For license details, refer to the associated LICENSE.md
// file or visit https://opensource.org/licenses/MIT

module Mit.Bindle.Server.Caching

open System
open System.Runtime.Caching

open Mit.Bindle.CsvReader
open Mit.Bindle.Scraper

let private scrapeKey = "bindle-scrape"

let private getCache () = MemoryCache.Default

let getScrape () : ScrapedPage option =
    let cache = getCache ()
    cache.Get(scrapeKey)
    |> Option.ofObj
    |> Option.map (fun found -> downcast found)

let storeScrape (page: ScrapedPage) (expires: DateTimeOffset) : unit =
    let cache = getCache ()
    cache.Set(scrapeKey, page, expires)

let getCsv (uri: Uri) : ParsedFile option =
    let cache = getCache ()
    cache.Get(uri.ToString())
    |> Option.ofObj
    |> Option.map (fun found -> downcast found)

let storeCsv (uri: Uri) (file: ParsedFile) (expires: DateTimeOffset) : unit =
    let cache = getCache ()
    cache.Set(uri.ToString(), file, expires)