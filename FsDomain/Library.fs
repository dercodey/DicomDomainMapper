namespace FsDomain

module Models =

    [<CLIMutable>]
    type DicomTag = {
        Group:int;
        Element:int;
    }

    [<CLIMutable>]
    type DicomUid = {
        UidString:string
    }

    [<CLIMutable>]
    type DicomAttribute = {
        DicomTag:DicomTag;
        Value:string;
    }

    type DicomInstance(sopInstanceUid:DicomUid, dicomAttributes:seq<DicomAttribute>) =
        // check that attributes are OK
        member val SopInstanceUid = sopInstanceUid
        member val DicomAttributes = dicomAttributes

    type DicomSeries(seriesInstanceUid:DicomUid) =
        // check that attributes are OK
        let mutable dicomInstances : list<DicomInstance> = [ ]
        member val SeriesInstanceUid = seriesInstanceUid
        member val DicomInstances = dicomInstances
        member this.AddInstance (dicomInstance:DicomInstance) =
            let seriesFromInstance = dicomInstance.DicomAttributes |> Seq.head
            if (seriesFromInstance.Value = seriesInstanceUid.ToString())
            then 
                dicomInstances <- dicomInstance :: dicomInstances
                dicomInstances |> List.length |> Result.Ok
            else
                "seriesInstanceUid mismatch" |> Result.Error