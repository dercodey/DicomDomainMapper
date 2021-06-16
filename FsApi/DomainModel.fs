module DomainModel

type ModelState =
| Valid
| Invalid of string

[<CLIMutable>]
type DicomTag = 
    { Group:int;
        Element:int; }
    member this.CheckModelState() =
        if this.Group < 0
        then Invalid "bad group"
        elif this.Element < 0
        then Invalid "bad element"
        else Valid

[<CLIMutable>]
type DicomUid = 
    { UidString:string; }
    override this.ToString () = this.UidString
    member this.CheckModelState() =
        if this.UidString.Contains("/")
        then Invalid "bad string"
        else Valid

[<CLIMutable>]
type DicomAttribute = 
    { DicomTag:DicomTag;
        Value:string; }
    member this.CheckModelState() =
        match this.DicomTag.CheckModelState() with
        | Invalid msg -> Invalid msg
        | Valid -> Valid

type Modality =
| CT
| DX

type DicomInstance(sopInstanceUid:DicomUid, dicomAttributes:seq<DicomAttribute>) =
    // check that attributes are OK
    member val SopInstanceUid = sopInstanceUid
    member val DicomAttributes = dicomAttributes

    member this.CheckModelState () = Valid

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
            dicomInstance 
            |> Result.Ok
        else
            "seriesInstanceUid mismatch" 
            |> Result.Error

    member this.CheckModelState () = Valid

    interface Seedwork.IAggregateRoot<DicomUid> with
        member val RootKey : DicomUid = Unchecked.defaultof<DicomUid>