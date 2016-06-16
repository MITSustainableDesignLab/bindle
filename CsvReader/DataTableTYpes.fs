// Copyright © 2016 Massachusetts Institute of Technology
// For license details, refer to the associated LICENSE.md
// file or visit https://opensource.org/licenses/MIT

namespace Mit.Bindle.CsvReader

open System
open System.IO

open CsvHelper

type MutableList<'T> = System.Collections.Generic.List<'T>

type Series = {
    Name: string
    Units: string
    Values: float []
    }
    with
        static member create (name: string) (units: string) (values: seq<float>) : Series =
            { Name = name; Units = units; Values = values |> Array.ofSeq }

type DataTable = {
    Series: Map<string, Series>
    Timestamps: DateTimeOffset []
    TimeDescription: string
    }
    with
        static member load (reader: TextReader) : DataTable =
            let rec append (existingTimestamps: MutableList<DateTimeOffset>) (existingData: MutableList<float> []) (parser: CsvParser) : unit =
                let isEof = ((=)  Unchecked.defaultof<string []>)

                let line = parser.Read()
                if isEof line then () else
                let timestamp = DateTimeOffset.Parse(line.[1])
                existingTimestamps.Add(timestamp)
                let newData = line.[2..]
                if newData.Length <> existingData.Length then
                    let rowNum = line.[0]
                    failwith (sprintf "Row %s had %i columns, but there were %i columns in the header row" rowNum newData.Length existingData.Length) else
                newData
                |> Seq.zip existingData
                |> Seq.iter (fun (existingVals, newVal) ->
                    let parsed = Double.Parse(newVal)
                    existingVals.Add(parsed))
                append existingTimestamps existingData parser

            let beforeComma (rawString: string) = rawString.Split(',').[0].Trim()
            let afterComma (rawString: string) = rawString.Split(',').[1].Trim()

            use parser = new CsvParser(reader)
            let header = parser.Read()
            let timeDesc = header.[1] |> afterComma
            let seriesNames = header.[2..] |> Array.map beforeComma
            let seriesValues = seriesNames |> Array.map (fun _ -> MutableList())
            let seriesUnits = header.[2..] |> Array.map afterComma
            let timestamps = MutableList<DateTimeOffset>()
            append timestamps seriesValues parser
            let series =
                seriesValues
                |> Seq.zip3 seriesNames seriesUnits
                |> Seq.map (fun (name, units, values) ->
                    let newSeries = Series.create name units values
                    (name, newSeries))
                |> Map.ofSeq
            { Series = series; Timestamps = Array.ofSeq timestamps; TimeDescription = timeDesc }