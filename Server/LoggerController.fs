// Copyright © 2016 Massachusetts Institute of Technology
// For license details, refer to the associated LICENSE.md
// file or visit https://opensource.org/licenses/MIT

namespace Mit.Bindle.Server.Controllers

open System
open System.Threading.Tasks
open System.Web.Http

open Mit.Bindle.Server

module BindleData = Mit.Bindle.Server.Data

[<RoutePrefix("api/logger")>]
type LoggerController () =
    inherit ApiController()

    member private this.OkForward x = this.Ok x

    [<Route("")>]
    member this.GetInfo() : Task<IHttpActionResult> =
        let ok x = this.OkForward x :> IHttpActionResult
        async {
            let! scrape = BindleData.getLatestScrape ()
            let res =
                {
                    LastConnectionTime = defaultArg scrape.LastConnectionTime DateTimeOffset.MinValue
                    NextConnectionTime = defaultArg scrape.NextConnectionTime DateTimeOffset.MaxValue
                }
            return ok res
        } |> Async.StartAsTask

