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

    [<HttpPost("patient/{patientId}/series")>]
    [<ProducesResponseType(200 (* HttpStatusCode.OK *))>]
    // [<ProducesResponseType(HttpStatusCode.Conflict)>]
    // [<ProducesResponseType(HttpStatusCode.BadRequest)>]
    [<ApiConventionMethod(typedefof<DefaultApiConventions>, nameof(DefaultApiConventions.Post))>]
    member this.AddDicomSeries (patientId:string, [<FromBody>] abDicomSeries:DicomSeries) =
        try
            $"Adding series for patient {patientId}" 
            |> logger.LogDebug

            abDicomSeries
            |> mapper.Map<DomainModel.DicomSeries>
            |> service.CreateSeriesAsync
            |> function task -> task.Wait()
            |> this.Ok
            :> ActionResult
        with
        | ex -> 
            ex.Message |> logger.LogError
            ex.Message 
            |> this.BadRequest 
            :> ActionResult