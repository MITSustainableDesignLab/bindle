// Copyright © 2016 Massachusetts Institute of Technology
// For license details, refer to the associated LICENSE.md
// file or visit https://opensource.org/licenses/MIT

[<AutoOpen>]
module Mit.Bindle.Server.Models

open System

open Mit.Bindle.CsvReader

type CsvHeaderSeriesInfo = Mit.Bindle.CsvReader.SeriesInfo

[<CLIMutable>]
type Entry = {
    Readings: Map<string, float>
    Measured: DateTimeOffset
    }
    with
        static member filterReadings (names: seq<string>) (entry: Entry) : Entry =
            let exclude = CaseInsensitiveStringSet names
            if exclude.Count = 0 then entry else
            let newReadings = Map.filter (fun name _ -> exclude.Contains name) entry.Readings
            { entry with Readings = newReadings }
        static member empty : Entry =
            {
                Readings = Map.empty
                Measured = DateTimeOffset.MinValue
            }

type LoggerInfo = {
    LastConnectionTime: DateTimeOffset
    NextConnectionTime: DateTimeOffset
    }
    
type SeriesInfo = {
    Name: string
    UnitName: string
    Units: string
    Type: string
    OMClassName: string
    OMPartNumber: string
    OMUnitIndex: int
    OMChannelType: string
    OMValuePattern: string
    LoggerDescription: string
    }
    with
        static member createFromHeader (header: CsvHeaderSeriesInfo) (units: string) : SeriesInfo =
            {
                Name = header.Name
                UnitName = header.UnitName
                Units = units
                Type = header.Type
                OMClassName = header.OMClassName
                OMPartNumber = header.OMPartNumber
                OMUnitIndex = header.OMUnitIndex
                OMChannelType = header.OMChannelType
                OMValuePattern = header.OMValuePattern
                LoggerDescription = header.Logger.LaunchDescription
            }

type SeriesValues = {
    Timestamps: DateTimeOffset []
    Values: Map<string, double []>
    }
    with
        static member create (timestamps: DateTimeOffset []) (values: Map<string, double []>) : SeriesValues =
            if values |> Map.exists (fun _ value -> value.Length <> timestamps.Length) then
                raise (ArgumentException("The timestamp count and value count must be equal"))
            { Timestamps = timestamps; Values = values }