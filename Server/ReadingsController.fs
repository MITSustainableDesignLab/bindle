// Copyright © 2016 Massachusetts Institute of Technology
// For license details, refer to the associated LICENSE.md
// file or visit https://opensource.org/licenses/MIT

namespace Mit.Bindle.Server.Controllers

open System
open System.Threading.Tasks
open System.Web.Http

open Mit.Bindle.Server

module BindleData = Mit.Bindle.Server.Data

[<RoutePrefix("api/readings")>]
type ReadingsController () =
    inherit ApiController()

    let filterNamesIfNotNull entry = function
        | null -> entry
        | names -> Entry.filterReadings names entry

    member private this.OkForward x = this.Ok x
    
    [<Route("all")>]
    member this.GetMostRecentEntry() : Task<IHttpActionResult> =
        let ok x = this.OkForward x :> IHttpActionResult
        async {
            let! entry = BindleData.getLatestEntry ()
            return ok entry
        } |> Async.StartAsTask
    
    [<Route("all")>]
    member this.GetSpecificEntry([<FromUri>] timestamp: DateTimeOffset) : Task<IHttpActionResult> =
        let ok x = this.OkForward x :> IHttpActionResult
        async {
            let! entry = BindleData.getLatestEntryBefore timestamp
            return ok entry
        } |> Async.StartAsTask

    [<Route("specific")>]
    member this.GetMostRecentEntry([<FromUri>] name: string[]) : Task<IHttpActionResult> =
        let ok x = this.OkForward x :> IHttpActionResult
        async {
            let! entry = BindleData.getLatestEntry ()
            return ok <| filterNamesIfNotNull entry name
        } |> Async.StartAsTask

    [<Route("specific")>]
    member this.GetSpecificEntry([<FromUri>] timestamp: DateTimeOffset, [<FromUri>] name: string[]) : Task<IHttpActionResult> =
        let ok x = this.OkForward x :> IHttpActionResult
        async {
            let! entry = BindleData.getLatestEntryBefore timestamp
            return ok <| filterNamesIfNotNull entry name
        } |> Async.StartAsTask