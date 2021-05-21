module Application

open System.Threading.Tasks

type IDicomApplicationService =         
    abstract member GetAllSeriesForPatient : string -> seq<DomainModel.DicomSeries> 
    abstract member CreateSeriesAsync : DomainModel.DicomSeries -> Task

type DicomApplicationService(repository:Repository.IAggregateRepository<DomainModel.DicomSeries, DomainModel.DicomUid>) = 
    interface IDicomApplicationService with
        member this.CreateSeriesAsync(arg1: DomainModel.DicomSeries): Task = 
            raise (System.NotImplementedException())
        member this.GetAllSeriesForPatient(arg1: string): seq<DomainModel.DicomSeries> = 
            raise (System.NotImplementedException())