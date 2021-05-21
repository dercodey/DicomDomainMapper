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

    let checkResult checkFunc result =
        if not(checkFunc result)
        then raise(Exception())
        else result

    let findTag tag (attributes:seq<DomainModel.DicomAttribute>) =
        attributes
        |> Seq.tryFind (fun da -> da.DicomTag.Equals(tag))
        |> function
            | None -> raise(Exception())
            | Some(da) -> da.Value

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
            let parsedAttributes =
                dicomStream
                |> parser.ParseStream
                |> checkResult ((findTag "DomainModel.DicomTag.SOPINSTANCEUID") >> ((=) seriesInstanceUid))

            { DomainModel.DicomUid.UidString = seriesInstanceUid }
            |> repository.GetAggregateForKey
            |> checkResult ((<>) Unchecked.defaultof<DomainModel.DicomSeries>)
            |> function 
                series ->
                    let sopInstanceUid =
                        { DomainModel.DicomUid.UidString = 
                            parsedAttributes
                            |> findTag "DomainModel.DicomTag.SOPINSTANCEUID" }

                    (sopInstanceUid, parsedAttributes)
                    |> DomainModel.DicomInstance
                    |> series.AddInstance |> ignore

                    series
                    |> repository.UpdateAsync |> ignore

                    sopInstanceUid
                    |> Task.FromResult
