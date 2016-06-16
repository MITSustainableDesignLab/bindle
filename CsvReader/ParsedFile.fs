// Copyright © 2016 Massachusetts Institute of Technology
// For license details, refer to the associated LICENSE.md
// file or visit https://opensource.org/licenses/MIT

namespace Mit.Bindle.CsvReader

open System
open System.IO
open System.Text.RegularExpressions

type ParsedFile = {
    Header: Header
    Data: DataTable
    }
    with
        static member load (reader: TextReader) : ParsedFile =
            let indicatesEndOfHeader line =
                let separator = Regex(@"^-+\s*$")
                line = Unchecked.defaultof<string> || separator.IsMatch(line)

            let rec extractHeaderLines (reader: TextReader) = seq {
                let line = reader.ReadLine()
                if not <| indicatesEndOfHeader line then
                    yield line
                    yield! extractHeaderLines reader
            }

            let headerLines = extractHeaderLines reader
            let headerText = String.Join(Environment.NewLine, headerLines)
            use headerReader = new StringReader(headerText)
            let header = Header.load headerReader
            let data = DataTable.load reader
            { Header = header; Data = data }