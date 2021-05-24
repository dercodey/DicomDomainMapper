module Abstractions

open System

[<CLIMutable>]
type DicomAttribute =
    { DicomTag:string;
        Value:string }

[<CLIMutable>]
type DicomInstance =
    { SopInstanceUid:string;
        DicomAttributes:seq<DicomAttribute>; }

[<CLIMutable>]
type DicomSeries = 
    { SeriesInstanceUid:string;
        PatientName:string;
        PatientId:string;
        Modality:string;
        AcquisitionDateTime:DateTime;
        ExpectedInstanceCount:int; }