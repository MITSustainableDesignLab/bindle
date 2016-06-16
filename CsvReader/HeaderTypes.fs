// Copyright © 2016 Massachusetts Institute of Technology
// For license details, refer to the associated LICENSE.md
// file or visit https://opensource.org/licenses/MIT

namespace Mit.Bindle.CsvReader

open System.IO

open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions

[<CLIMutable>]
type DataFormat = {
    DateTimeDelimiter: string
    DecimalSeparator: string
    DataDelimiter: string
    DateFormat: string
    TimeFormat: string
    GmtOffset: int
    DaylightSavings: bool
    }

[<CLIMutable>]
type LoggerInfo = {
    LaunchDescription: string
    Model: string
    Vendor: string
    SerialNumber: string
    MemorySize: int
    FirmwareVersion: string
    FirmwareVersionRaw: string
    DeploymentNumber: string
    }

[<CLIMutable>]
type SeriesInfo = {
    Name: string
    Type: string
    UnitName: string
    [<YamlMember(Alias="OMClassName")>] OMClassName: string
    [<YamlMember(Alias="OMPartNumber")>] OMPartNumber: string
    [<YamlMember(Alias="OMUnitIndex")>] OMUnitIndex: int
    [<YamlMember(Alias="OMChannelType")>] OMChannelType: string
    [<YamlMember(Alias="OMValuePattern")>] OMValuePattern: string
    Logger: LoggerInfo
    }

[<CLIMutable>]
type Header = {
    [<YamlMember(Alias="Data Format")>] DataFormat: DataFormat
    [<YamlMember(Alias="Logger Info")>] LoggerInfo: LoggerInfo []
    [<YamlMember(Alias="Series Info")>] SeriesInfo: SeriesInfo []
    }
    with
        static member load (reader: TextReader) : Header =
            let deserializer = Deserializer(namingConvention=CamelCaseNamingConvention())
            deserializer.Deserialize<Header>(reader)
