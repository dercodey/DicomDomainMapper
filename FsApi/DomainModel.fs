module DomainModel

[<CLIMutable>]
type DicomTag = 
    { Group:int;
        Element:int; }

[<CLIMutable>]
type DicomUid = 
    { UidString:string; }
    override this.ToString () = this.UidString

[<CLIMutable>]
type DicomAttribute = 
    { DicomTag:DicomTag;
        Value:string; }

type Modality =
| CT
| DX

type DicomInstance(sopInstanceUid:DicomUid, dicomAttributes:seq<DicomAttribute>) =
    // check that attributes are OK
    member val SopInstanceUid = sopInstanceUid
    member val DicomAttributes = dicomAttributes

    interface Seedwork.IEntity<DicomUid> with
        member val Key : DicomUid = Unchecked.defaultof<DicomUid>

type DicomSeries(seriesInstanceUid:DicomUid, 
        patientName:string, patientId:string, 
        modality:Modality, 
        acquisitionDateTime:System.DateTime, 
        expectedInstanceCount:int,
        dicomInstances:seq<DicomInstance>) =

    // check that attributes are OK
    let mutable dicomInstances = dicomInstances |> List.ofSeq
    member val SeriesInstanceUid = seriesInstanceUid
    member val PatientName = patientName
    member val PatientId = patientId
    member val AcquisitionDateTime = acquisitionDateTime
    member val ExpectedInstanceCount = expectedInstanceCount
    member val DicomInstances = dicomInstances
    member this.AddInstance (dicomInstance:DicomInstance) =
        let seriesFromInstance = dicomInstance.DicomAttributes |> Seq.head
        if (seriesFromInstance.Value = seriesInstanceUid.ToString())
        then 
            dicomInstances <- dicomInstance :: dicomInstances
            dicomInstances |> List.length |> Result.Ok
        else
            "seriesInstanceUid mismatch" |> Result.Error

    interface Seedwork.IAggregateRoot<DicomUid> with
        member val RootKey : DicomUid = Unchecked.defaultof<DicomUid>