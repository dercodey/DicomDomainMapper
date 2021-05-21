module Application

open System.Threading.Tasks
open System.IO

type IDicomParser =
    abstract member ParseStream : Stream -> seq<DomainModel.DicomAttribute>

type IDicomApplicationService =         
    abstract member GetSeriesByUid : string -> 
        DomainModel.DicomSeries
    abstract member CreateSeriesAsync : DomainModel.DicomSeries -> 
        Task
    abstract member AddInstanceFromStreamAsync : string -> Stream -> 
        Task<DomainModel.DicomUid>

type DicomParser() =    
    interface IDicomParser with
        member this.ParseStream(arg1: Stream): seq<DomainModel.DicomAttribute> = 
            raise (System.NotImplementedException())

type DicomApplicationService(repository:Repository.IDicomSeriesRepository, 
                                parser:IDicomParser) = 
    interface IDicomApplicationService with
        member this.CreateSeriesAsync (dmDicomSeries) =
            dmDicomSeries
            |> repository.UpdateAsync

        member this.GetSeriesByUid (seriesInstanceUid) = 
#if USE_ACTUAL
            { DomainModel.DicomUid.UidString=seriesInstanceUid }
            |> repository.GetAggregateForKey
#else
            DomainModel.DicomSeries(
                seriesInstanceUid = { DomainModel.DicomUid.UidString=seriesInstanceUid },
                patientName = "Last, First",
                patientId = "98765",
                modality = DomainModel.Modality.CT,
                acquisitionDateTime = System.DateTime.Now,
                expectedInstanceCount = 3,
                dicomInstances = [])
#endif

        member this.AddInstanceFromStreamAsync(seriesInstanceUid: string) (dicomStream: Stream): Task<DomainModel.DicomUid> =             
            let parsedElements =
                parser.ParseStream(dicomStream)

            // check that the series has already been created
            let extractedSeriesInstanceUid = 
                parsedElements
                |> Seq.find (fun da -> 
                        da.DicomTag.Equals("DomainModel.DicomTag.SOPINSTANCEUID"))
            if (extractedSeriesInstanceUid.Value <> seriesInstanceUid)
            then 
                null
            else
                raise (System.NotImplementedException())
