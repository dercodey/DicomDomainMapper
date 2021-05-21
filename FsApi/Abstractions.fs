module Abstractions

open System

[<CLIMutable>]
type DicomSeries = {
    SeriesInstanceUid:string;
    PatientName:string;
    PatientId:string;
    Modality:string;
    AcquisitionDateTime:DateTime;
    ExpectedInstanceCount:int;
}