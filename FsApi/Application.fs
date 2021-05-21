module Application

open System.Threading.Tasks
open System.IO
open System

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
                dicomStream
                |> parser.ParseStream

            parsedElements
            |> Seq.tryFind (fun da -> 
                    da.DicomTag.Equals("DomainModel.DicomTag.SOPINSTANCEUID"))
            |> function
                | None -> 
                    raise(Exception())
                | Some(extractedSeriesInstanceUid) 
                        when extractedSeriesInstanceUid.Value <> seriesInstanceUid ->
                    raise(Exception())

            let existingSeries = 
                { DomainModel.DicomUid.UidString = seriesInstanceUid }
                |> repository.GetAggregateForKey
                |> function
                    //| null ->
                    //    raise(InvalidOperationException("SeriesInstanceUID not found in repository"))
                    | existingSeries -> existingSeries

            
            let sopInstanceUid =
                parsedElements
                |> Seq.tryFind (fun da -> 
                    da.DicomTag.Equals("DomainModel.DicomTag.SOPINSTANCEUID"))
                |> function
                    | None -> 
                        raise(Exception())
                    | Some(sopInstanceUid) ->
                        { DomainModel.DicomUid.UidString = sopInstanceUid.Value }

            (sopInstanceUid, parsedElements)
            |> DomainModel.DicomInstance
            |> existingSeries.AddInstance
            |> ignore

            existingSeries
            |> repository.UpdateAsync
            |> ignore

            sopInstanceUid
            |> Task.FromResult

