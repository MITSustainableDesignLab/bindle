namespace Mit.Bindle.Server

open System
open System.Net.Http
open System.Web
open System.Web.Http
open System.Web.Routing

open Newtonsoft.Json
open Newtonsoft.Json.Serialization

type ReadingsRoute = { controller: string }
type SeriesRoute = { controller: string; particulars: string }

type Global() =
    inherit System.Web.HttpApplication() 

    static member RegisterWebApi(config: HttpConfiguration) =
        // Configure routing
        config.MapHttpAttributeRoutes()
        
        // Configure serialization
        config.Formatters.XmlFormatter.UseXmlSerializer <- true
        config.Formatters.JsonFormatter.SerializerSettings.ContractResolver <- CamelCasePropertyNamesContractResolver()
        config.Formatters.JsonFormatter.SerializerSettings.Formatting <- Formatting.Indented
        config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling <- NullValueHandling.Ignore

        // Additional Web API settings

    member x.Application_Start() =
        GlobalConfiguration.Configure(Action<_> Global.RegisterWebApi)
