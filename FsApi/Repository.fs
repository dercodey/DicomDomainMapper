module Repository

open AutoMapper
open AutoMapper.EntityFrameworkCore

type IAggregateRepository<'entity, 'key> = 
    abstract member GetAggregateForKey : 'key -> 
        option<'entity>
    abstract member UpdateAsync : 'entity -> 
        Async<Result<'key, string>>
    abstract member RemoveAsync : 'key -> 
        Async<Result<'key, string>>

type IDicomSeriesRepository = 
    IAggregateRepository<DomainModel.DicomSeries, DomainModel.DicomUid>

type DicomSeriesRepository(context:EFModel.DicomDbContext, mapper:IMapper) = 
    interface IDicomSeriesRepository with
        member this.GetAggregateForKey(dmDicomUid) = 
            context.DicomSeries
            |> Seq.tryFind (fun series -> 
                series.SeriesInstanceUid = dmDicomUid.UidString)
            |> Option.map mapper.Map<DomainModel.DicomSeries>

        member this.UpdateAsync(dmDicomSeries) = 
            async {
                match dmDicomSeries.CheckModelState() with
                | DomainModel.ModelState.Invalid msg -> 
                    return Error msg

                | DomainModel.ModelState.Valid ->
                    let! updatedSeries = 
                        dmDicomSeries
                        |> mapper.Map<EFModel.DicomSeries>
                        |> context.DicomSeries.Persist(mapper).InsertOrUpdateAsync
                        |> Async.AwaitTask

                    if updatedSeries.SeriesInstanceUid <> dmDicomSeries.SeriesInstanceUid.ToString()
                    then 
                        return Error "problem with update"
                    else 
                        return Ok dmDicomSeries.SeriesInstanceUid
            }

        member this.RemoveAsync(dmSeriesInstanceUid: DomainModel.DicomUid): Async<Result<DomainModel.DicomUid,string>> = 

            async {
                do! dmSeriesInstanceUid
                    |> (this:>IDicomSeriesRepository).GetAggregateForKey
                    |> mapper.Map<EFModel.DicomSeries>
                    |> context.DicomSeries.Persist(mapper).RemoveAsync
                    |> Async.AwaitTask
                return Ok dmSeriesInstanceUid
            }
