namespace FsApi.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open FsApi
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
type DicomController ((*service:IDicomApplicationService,*) logger : ILogger<DicomController>) =
    inherit ControllerBase()

    let summaries = [| "Freezing"; "Bracing"; "Chilly"; "Cool"; "Mild"; "Warm"; "Balmy"; "Hot"; "Sweltering"; "Scorching" |]

    [<HttpGet>]
    member __.Get() : WeatherForecast[] =
        let rng = System.Random()
        [|
            for index in 0..4 ->
                { Date = DateTime.Now.AddDays(float index)
                  TemperatureC = rng.Next(-20,55)
                  Summary = summaries.[rng.Next(summaries.Length)] }
        |]

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