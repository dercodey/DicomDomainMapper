module Application

open System.Threading.Tasks

type IDicomApplicationService =         
    abstract member GetSeriesByUid : string -> DomainModel.DicomSeries
    abstract member CreateSeriesAsync : DomainModel.DicomSeries -> Task

type DicomApplicationService(repository:Repository.IAggregateRepository<DomainModel.DicomSeries, DomainModel.DicomUid>) = 
    interface IDicomApplicationService with

        member this.CreateSeriesAsync(arg1: DomainModel.DicomSeries): Task = 
            raise (System.NotImplementedException())

        member this.GetSeriesByUid (seriresInstanceUid: string): DomainModel.DicomSeries = 
            DomainModel.DicomSeries(
                seriesInstanceUid = { DomainModel.DicomUid.UidString="1.2.3.9" },
                patientName = "",
                patientId = "",
                modality = DomainModel.Modality.CT,
                acquisitionDateTime = System.DateTime.Now,
                expectedInstanceCount = 3,
                dicomInstances = [])