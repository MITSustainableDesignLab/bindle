// Copyright © 2016 Massachusetts Institute of Technology
// For license details, refer to the associated LICENSE.md
// file or visit https://opensource.org/licenses/MIT

module Mit.Bindle.Server.Data

open System
open System.IO
open System.Net
open System.Web.Configuration

open Mit.Bindle.CsvReader
open Mit.Bindle.Scraper

let private hoboLinkUriKey = "HobolinkUri"

let private createWebClient () =
    new CookieAwareWebClient()

let getLatestScrape () : Async<ScrapedPage> =
    match Caching.getScrape () with
    | Some(cachedScrape) -> async { return cachedScrape }
    | None -> async {
        let hobolinkUri = Uri <| WebConfigurationManager.AppSettings.[hoboLinkUriKey]
        let! page' = Scrape.getScrapedPageAsync hobolinkUri
        page'.NextConnectionTime |> Option.iter (Caching.storeScrape page')
        return page'
    }

let private getCsv (uri: Uri) (cookies: CookieContainer) (cacheAndExpireAt: DateTimeOffset option) : Async<ParsedFile> =
    let retrieveCsv uri cookies = async {
        use client = createWebClient ()
        client.Cookies <- cookies
        let! contents = client.AsyncDownloadString(uri)
        use reader = new StringReader(contents)
        return ParsedFile.load reader
    }
    async {
        match Caching.getCsv uri with
        | Some csv -> return csv
        | None ->
            let! csv = retrieveCsv uri cookies
            cacheAndExpireAt |> Option.iter (Caching.storeCsv uri csv)
            return csv
    }

let getReferencedCsv (scrape: ScrapedPage) (uri: Uri) : Async<ParsedFile> =
    getCsv uri scrape.Cookies scrape.NextConnectionTime

let private findInCsv (csv: ParsedFile) (timestamp: DateTimeOffset) : Entry option =
    let isBefore targetTimestamp rowTimestamp = rowTimestamp <= targetTimestamp
    csv.Data.Timestamps
    |> Array.tryFindIndexBack (isBefore timestamp)
    |> Option.map (fun index ->
        let matchTimestamp = csv.Data.Timestamps.[index]
        let readings = csv.Data.Series |> Map.map (fun _ series -> series.Values.[index])
        { Readings = readings; Measured = matchTimestamp })

let getLatestEntryBefore (timestamp: DateTimeOffset) : Async<Entry> = 
    async {
        let! scrape = getLatestScrape ()
        let rec findFirstContaining uris = async {
            match uris with
            | [] -> return Entry.empty
            | first :: rest ->
                let! csv = getReferencedCsv scrape first
                System.Diagnostics.Debug.WriteLine("Checking {0}...", first)
                match findInCsv csv timestamp with
                | Some matchingEntry ->
                    System.Diagnostics.Debug.WriteLine("Found!")
                    return matchingEntry
                | None ->
                    System.Diagnostics.Debug.WriteLine("Not found.")
                    return! findFirstContaining rest
        }
        return! findFirstContaining scrape.CsvUris
    }

let getLatestEntry () : Async<Entry> =
    getLatestEntryBefore DateTimeOffset.MaxValue