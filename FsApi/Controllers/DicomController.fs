namespace FsApi.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open AutoMapper
open Application
open Abstractions

[<ApiController>]
[<Route("[controller]")>]
type DicomController (service:IDicomApplicationService, mapper:IMapper, logger:ILogger<DicomController>) =
    inherit ControllerBase()

    let logError msg =
        logger.LogError msg
        msg

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
            ex.Message 
            |> logError
            |> this.BadRequest 
            :> ActionResult

    [<HttpPost("series/{seriesUid}/instances")>]
    [<ProducesResponseType(200 (* HttpStatusCode.OK *))>]
    //[<ProducesResponseType(HttpStatusCode.Conflict)>]
    //[<ProducesResponseType(HttpStatusCode.BadRequest)>]
    [<ApiConventionMethod(typedefof<DefaultApiConventions>, nameof(DefaultApiConventions.Post))>]
    member this.AddDicomInstance(seriesInstanceUid:string, dicomFile:IFormFile) : Task<ActionResult> =
        match dicomFile with
        | null ->
            "file is null or empty."
            |> logError
            |> this.BadRequest 
            :> ActionResult
        | dicomFile ->
            try
                dicomFile.OpenReadStream()
                |> service.AddInstanceFromStreamAsync seriesInstanceUid 
                |> function
                    | null -> 
                        "mismatched series instance UID"
                        |> logError
                        |> this.BadRequest
                        :> ActionResult
                    | addedSopInstanceUid ->
                        addedSopInstanceUid.ToString()
                        |> this.Ok
                        :> ActionResult 
            with
            | ex when ex.Message.EndsWith("already exists.") ->
                ex.Message
                |> logError
                |> this.Conflict
                :> ActionResult 
            | ex ->
                ex.Message
                |> logError
                |> this.BadRequest
                :> ActionResult 
        |> Task.FromResult
