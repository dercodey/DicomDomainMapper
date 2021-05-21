namespace FsApi.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open FsDomain.Application

module Abstractions = 
    type DicomSeries = {
        SeriesInstanceUid:string;
        PatientName:string;
        PatientId:string;
        Modality:string;
        AcquisitionDateTime:DateTime;
        ExpectedInstanceCount:int;
    }

[<ApiController>]
[<Route("[controller]")>]
type DicomController (service:IDicomApplicationService, logger : ILogger<DicomController>) =
    inherit ControllerBase()

    [<HttpGet("patient/{patientId}/series")>]
    [<ProducesResponseType(200 (* HttpStatusCode.OK *))>]
    [<ProducesResponseType(404 (* HttpStatusCode.NotFound *))>]
    [<ApiConventionMethod(typedefof<DefaultApiConventions>, nameof(DefaultApiConventions.Get))>]
    member this.GetAllDicomSeriesForPatient (patientId:string) =
        let seriesForPatient = [

            { Abstractions.DicomSeries.SeriesInstanceUid="1.2.3.9";
                Abstractions.DicomSeries.PatientName="";
                Abstractions.DicomSeries.PatientId="";
                Abstractions.DicomSeries.Modality="CT";
                Abstractions.DicomSeries.AcquisitionDateTime=DateTime.Now;
                Abstractions.DicomSeries.ExpectedInstanceCount=3; },

            { Abstractions.DicomSeries.SeriesInstanceUid="1.2.3.11";
                Abstractions.DicomSeries.PatientName="";
                Abstractions.DicomSeries.PatientId="";
                Abstractions.DicomSeries.Modality="CT";
                Abstractions.DicomSeries.AcquisitionDateTime=DateTime.Now;
                Abstractions.DicomSeries.ExpectedInstanceCount=3; }
        ]
        this.Ok(seriesForPatient) :> ActionResult