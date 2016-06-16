// Copyright © 2016 Massachusetts Institute of Technology
// For license details, refer to the associated LICENSE.md
// file or visit https://opensource.org/licenses/MIT

module Mit.Bindle.Scraper.Scrape

open System
open System.IO
open System.Net
open System.Text.RegularExpressions

open HtmlAgilityPack

let private csvHrefPattern = @"[^&]+\.csv" 
let private csvHrefRegex = Regex(csvHrefPattern)
let private latestConnectionsIdFragment = "latest-connections-table-id_data"
let private linksPath = "//a[@href]"
let private nextConnectionStringRegex = Regex("Next connection expected (?<minutes>\d+) minute")

let private getPageStreamAsync (uri: Uri) : Async<Stream * CookieContainer> = async {
    use client = new CookieAwareWebClient()
    let! stream = client.OpenReadTaskAsync(uri) |> Async.AwaitTask
    return (stream, client.Cookies)
}

let private getPageDocumentAsync (uri: Uri) : Async<HtmlDocument * CookieContainer> = async {
    let! stream, cookies = getPageStreamAsync uri
    let doc = HtmlDocument()
    doc.Load(stream)
    return (doc, cookies)
}

let private relativeCsvUris (doc: HtmlDocument) : seq<string> =
    doc.DocumentNode.SelectNodes(linksPath)
    |> Seq.collect (fun node -> node.ChildAttributes("href"))
    |> Seq.map (fun att -> att.Value)
    |> Seq.where csvHrefRegex.IsMatch
    |> Seq.map WebUtility.HtmlDecode

let private appendRelativeUriToRoot (root: Uri) (relative: string) : Uri =
    Uri(root, relative)

let private hasConnectionsIdFragment (node: HtmlNode) : bool =
    let atts = node.Attributes
    atts.Count > 0 && atts.["id"].Value.Contains(latestConnectionsIdFragment)

let private getTime (raw: string) : DateTimeOffset option =
    let pattern = @"\d{2}:\d{2}"
    let m = Regex.Match(raw, pattern)
    if not m.Success then None else
    let time = DateTimeOffset.Parse(m.Value)
    Some time

let private getConnectionTableCellTexts (doc: HtmlDocument) : seq<string> =
    doc.DocumentNode.SelectNodes(".//tbody")
    |> Seq.where hasConnectionsIdFragment
    |> Seq.exactlyOne
    |> fun node -> node.SelectNodes(".//td")
    |> Seq.map (fun node -> node.InnerText)

let private getLastConnectionTime (doc: HtmlDocument) : DateTimeOffset option =
    getConnectionTableCellTexts doc
    |> Seq.choose getTime
    |> Seq.tryHead

let private getNextConnectionTime (doc: HtmlDocument) : DateTimeOffset option =
    getConnectionTableCellTexts doc
    |> Seq.choose (fun contents ->
        let m = nextConnectionStringRegex.Match(contents)
        if not m.Success then None else
        let parsedOk, parsed = Double.TryParse(m.Groups.["minutes"].Value)
        if parsedOk then Some parsed else None)
    |> Seq.truncate 1
    |> Seq.tryHead
    |> Option.map DateTimeOffset.Now.AddMinutes

let getScrapedPageAsync (uri: Uri) : Async<ScrapedPage> = async {
    let! doc, cookies = getPageDocumentAsync uri
    let csvUris = relativeCsvUris doc
    let root = Uri(uri.GetLeftPart(UriPartial.Authority))
    let absoluteCsvUris = csvUris |> Seq.map (appendRelativeUriToRoot root) |> List.ofSeq
    let lastConnectionTime = getLastConnectionTime doc
    let nextConnectionTime = getNextConnectionTime doc
    return ScrapedPage.create uri absoluteCsvUris lastConnectionTime nextConnectionTime cookies
}