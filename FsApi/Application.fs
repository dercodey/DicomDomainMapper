module Application

open System.Threading.Tasks
open System.IO
open System

type IDicomParser =
    abstract member ParseStreamAsync : Stream -> Async<seq<DomainModel.DicomAttribute>>

type IDicomApplicationService =         
    abstract member GetSeriesByUid : string -> 
        option<DomainModel.DicomSeries>
    abstract member CreateSeriesAsync : DomainModel.DicomSeries -> 
        Async<Result<DomainModel.DicomUid, string>>
    abstract member AddInstanceFromStreamAsync : string -> Stream -> 
        Async<Result<DomainModel.DicomUid, string>>

type DicomParser() =    
    interface IDicomParser with
        member this.ParseStreamAsync(stream: Stream): Async<seq<DomainModel.DicomAttribute>> = 
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
#if TEST_VALUE
            DomainModel.DicomSeries(
                seriesInstanceUid = { DomainModel.DicomUid.UidString=seriesInstanceUid },
                patientName = "Last, First",
                patientId = "98765",
                modality = DomainModel.Modality.CT,
                acquisitionDateTime = System.DateTime.Now,
                expectedInstanceCount = 3,
                dicomInstances = []) 
            |> Some
#else
            { DomainModel.DicomUid.UidString=seriesInstanceUid }
            |> repository.GetAggregateForKey
#endif

        member this.AddInstanceFromStreamAsync(seriesInstanceUid: string) (dicomStream: Stream) =
            async {
                let! parsedAttributes =
                    dicomStream
                    |> parser.ParseStreamAsync

                return parsedAttributes
                    |> findTag "DomainModel.DicomTag.SOPINSTANCEUID"
                    |> (<>) seriesInstanceUid
                    |> function
                        | true -> Error "no SOP instance UID"
                        | false -> 
                            { DomainModel.DicomUid.UidString = seriesInstanceUid }
                            |> repository.GetAggregateForKey
                            |> function
                                | None -> Error "series not found"                            
                                | Some(dicomSeries) ->
                                    let sopInstanceUid =
                                        { DomainModel.DicomUid.UidString = 
                                            parsedAttributes
                                            |> findTag "DomainModel.DicomTag.SOPINSTANCEUID" }

                                    (sopInstanceUid, parsedAttributes)
                                    |> DomainModel.DicomInstance
                                    |> dicomSeries.AddInstance |> ignore

                                    dicomSeries
                                    |> repository.UpdateAsync |> ignore

                                    Ok sopInstanceUid
            }
