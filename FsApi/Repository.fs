module Repository

open System.Threading.Tasks
open AutoMapper
open AutoMapper.EntityFrameworkCore

type IAggregateRepository<'entity, 'key> = 
    abstract member GetAggregateForKey : 'key -> 'entity
    abstract member UpdateAsync : 'entity -> Task

type IDicomSeriesRepository = 
    IAggregateRepository<DomainModel.DicomSeries, DomainModel.DicomUid>

type DicomSeriesRepository(context:EFModel.DicomDbContext, mapper:IMapper) = 
    interface IDicomSeriesRepository with
        member this.GetAggregateForKey(dmDicomUid) = 
            context.DicomSeries
            |> Seq.find (fun series -> 
                series.SeriesInstanceUid = dmDicomUid.UidString)
            |> mapper.Map<DomainModel.DicomSeries>

        member this.UpdateAsync(dmDicomSeries) = 
            dmDicomSeries
            |> mapper.Map<EFModel.DicomSeries>
            |> context.DicomSeries.Persist(mapper).InsertOrUpdateAsync
            :> Task
