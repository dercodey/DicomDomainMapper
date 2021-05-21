module Application

open System.Threading.Tasks

type IDicomApplicationService =         
    abstract member GetSeriesByUid : string -> DomainModel.DicomSeries
    abstract member CreateSeriesAsync : DomainModel.DicomSeries -> Task

type DicomApplicationService(repository:Repository.IAggregateRepository<DomainModel.DicomSeries, DomainModel.DicomUid>) = 
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