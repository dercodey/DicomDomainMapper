module Repository

open System.Threading.Tasks
open AutoMapper
open AutoMapper.EntityFrameworkCore

type IAggregateRepository<'entity, 'key> = 
    abstract member GetAggregateForKey : 'key -> option<'entity>
    abstract member UpdateAsync : 'entity -> Async<Result<'key, string>>

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
                let! updated = 
                    dmDicomSeries
                    |> mapper.Map<EFModel.DicomSeries>
                    |> context.DicomSeries.Persist(mapper).InsertOrUpdateAsync
                    |> Async.AwaitTask
                return
                    Ok dmDicomSeries.SeriesInstanceUid
            }
