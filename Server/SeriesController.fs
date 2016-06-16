// Copyright © 2016 Massachusetts Institute of Technology
// For license details, refer to the associated LICENSE.md
// file or visit https://opensource.org/licenses/MIT

namespace Mit.Bindle.Server.Controllers

open System
open System.Threading.Tasks
open System.Web.Http

open Mit.Bindle.Server

module BindleData = Mit.Bindle.Server.Data

type Csv = Mit.Bindle.CsvReader.ParsedFile
type CsvHeaderSeriesInfo = Mit.Bindle.CsvReader.SeriesInfo

[<RoutePrefix("api/series")>]
type SeriesController () as this =
    inherit ApiController()

    let notFound = this.NotFoundForward

    [<Route("info/{name}")>]
    member this.GetInfo (name: string) : Task<IHttpActionResult> =
        async {
            let ok x = this.OkForward x
            let! scrape = BindleData.getLatestScrape ()
            match List.tryHead scrape.CsvUris with
            | None -> return notFound ()
            | Some uri ->
                let! csv = BindleData.getReferencedCsv scrape uri
                let units seriesName = csv.Data.Series.[seriesName].Units
                let knownSeries = csv.Header.SeriesInfo
                let matchingName (s: CsvHeaderSeriesInfo) = s.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)
                match knownSeries |> Array.tryFind matchingName with
                | None -> return notFound ()
                | Some(s) -> return ok <| SeriesInfo.createFromHeader s (units s.Name)
        } |> Async.StartAsTask

    [<Route("values")>]
    member this.GetValues ([<FromUri>] name: string[]) : Task<IHttpActionResult> =
        let ok x = this.OkForward x
        let nameMatches =
            let names =
                match name with | null -> [||] | names -> names
                |> CaseInsensitiveStringSet
            names.Contains
        let fromCsv (csv: Csv) =
            let values =
                csv.Data.Series
                |> Map.toSeq
                |> Seq.filter (fst >> nameMatches)
                |> Seq.map (fun (name, series) -> (name, series.Values))
                |> Map.ofSeq
            (csv.Data.Timestamps, values)
        let addNaNDummies forNames toMap =
            let seriesLength =
                let lengths =
                    toMap
                    |> Map.toSeq
                    |> Seq.map (snd >> Array.length)
                    |> Seq.distinct
                    |> List.ofSeq
                if lengths.Length > 1 then raise (ArgumentException("The map has values of unequal length")) else
                lengths.Head
            let namesMatch (a: string) (b: string) = a.Equals(b, StringComparison.CurrentCultureIgnoreCase)
            let alreadyExists name m = m |> Map.tryFindKey (fun k _ -> namesMatch k name) |> Option.isSome
            forNames |> Seq.fold (fun m name ->
                if alreadyExists name m then m else
                let newValues = Array.create seriesLength Double.NaN
                Map.add name newValues m) toMap
        let appendCsvData first second =
            let keys = Map.toSeq >> Seq.map fst
            let newFirst = addNaNDummies (keys second) first
            let newSecond = addNaNDummies (keys first) second
            newSecond |> Map.fold (fun m k v ->
                let newValues = Array.append v newFirst.[k]
                Map.add k newValues m) newFirst
        async {
            let! scrape = BindleData.getLatestScrape ()
            let rec createFromCsvs timestampsSoFar valuesSoFar uris = async {
                match uris with
                | [] -> return SeriesValues.create timestampsSoFar valuesSoFar
                | uri :: rest ->
                    let! csv = BindleData.getReferencedCsv scrape uri
                    let newTimestamps, newValues = fromCsv csv
                    let timestamps = Array.append newTimestamps timestampsSoFar
                    let values =
                        if Map.isEmpty valuesSoFar then newValues
                        else appendCsvData newValues valuesSoFar
                    return! createFromCsvs timestamps values rest
            }
            let! results = createFromCsvs [||] Map.empty scrape.CsvUris
            return ok results
        } |> Async.StartAsTask

    member private this.OkForward<'T> (x: 'T) = this.Ok x :> IHttpActionResult
    member private this.NotFoundForward () = this.NotFound () :> IHttpActionResult
