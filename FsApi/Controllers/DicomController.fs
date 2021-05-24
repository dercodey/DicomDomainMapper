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
type DicomController (service:IDicomApplicationService, mapper:IMapper, 
        logger:ILogger<DicomController>) as this =
    inherit ControllerBase()

    let controllerBase = this :> ControllerBase
    let ok result = result |> controllerBase.Ok :> ActionResult
    let notFound id = id |> controllerBase.NotFound :> ActionResult
    let badRequest (info:obj) = id |> controllerBase.BadRequest :> ActionResult
    let conflict (info:obj) = id |> controllerBase.Conflict :> ActionResult

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
        |> ok

    [<HttpPost("patient/{patientId}/series")>]
    [<ProducesResponseType(200 (* HttpStatusCode.OK *))>]
    // [<ProducesResponseType(HttpStatusCode.Conflict)>]
    // [<ProducesResponseType(HttpStatusCode.BadRequest)>]
    [<ApiConventionMethod(typedefof<DefaultApiConventions>, nameof(DefaultApiConventions.Post))>]
    member this.AddDicomSeriesAsync (patientId:string, [<FromBody>] abDicomSeries:DicomSeries) =
        try
            $"Adding series for patient {patientId}" 
            |> logger.LogDebug

            async {
                let! result = 
                    abDicomSeries
                    |> mapper.Map<DomainModel.DicomSeries>
                    |> service.CreateSeriesAsync

                return 
                    match result with
                    | Ok result -> 
                        result 
                        |> ok
                    | Error msg -> 
                        msg 
                        |> badRequest
            } |> Async.StartAsTask
        with
        | ex -> 
            ex.Message 
            |> logError
            |> badRequest
            |> Task.FromResult

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
            |> badRequest 
            |> Task.FromResult

        | dicomFile ->
            try
                async {
                    let! result =
                        dicomFile.OpenReadStream()
                        |> service.AddInstanceFromStreamAsync seriesInstanceUid 

                    return 
                        match result with
                        | Error msg ->
                            msg
                            |> logError
                            |> badRequest
                        | Ok addedSopInstanceUid ->
                            addedSopInstanceUid.ToString()
                            |> ok
                } |> Async.StartAsTask

            with
            | ex when ex.Message.EndsWith("already exists.") ->
                ex.Message
                |> logError
                |> conflict
                |> Task.FromResult

            | ex ->
                ex.Message
                |> logError
                |> badRequest
                |> Task.FromResult
