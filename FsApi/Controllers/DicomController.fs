namespace FsApi.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open AutoMapper
open Application
open Abstractions

[<ApiController>]
[<Route("[controller]")>]
type DicomController (service:IDicomApplicationService, mapper:IMapper, logger:ILogger<DicomController>) =
    inherit ControllerBase()

    [<HttpGet("patient/{patientId}/series/{seriesInstanceUid}")>]
    [<ProducesResponseType(200 (* HttpStatusCode.OK *))>]
    [<ProducesResponseType(404 (* HttpStatusCode.NotFound *))>]
    [<ApiConventionMethod(typedefof<DefaultApiConventions>, nameof(DefaultApiConventions.Get))>]
    member this.GetDicomSeriesByUid (patientId:string) (seriesInstanceUid:string) =
        seriesInstanceUid
        |> service.GetSeriesByUid
        |> mapper.Map<Abstractions.DicomSeries>
        |> this.Ok
        :> ActionResult