module Repository

open System.Threading.Tasks

type IAggregateRepository<'entity, 'key> = 
    abstract member GetAggregateForKey : 'key -> 'entity
    abstract member UpdateAsync : 'entity -> Task

let mapSeriesEfToDomainModel (efSeries:EFModel.DicomSeries) =
    efSeries.DicomInstances 
    |> Seq.map (fun efInstance -> 
        efInstance.DicomAttributes
        |> Seq.map (fun efAttribute -> 
            { DomainModel.DicomAttribute.DicomTag = Unchecked.defaultof<DomainModel.DicomTag>;
                DomainModel.DicomAttribute.Value = "string" })
        |> function 
            efAttributes ->
                DomainModel.DicomInstance(
                    sopInstanceUid = { DomainModel.DicomUid.UidString = efInstance.SopInstanceUid },
                    dicomAttributes = efAttributes))
    |> function 
        efInstances ->
            DomainModel.DicomSeries(
                seriesInstanceUid = { UidString = efSeries.SeriesInstanceUid },
                patientName = efSeries.PatientName, 
                patientId = efSeries.PatientId,
                modality = DomainModel.Modality.CT, 
                acquisitionDateTime = efSeries.AcquistionDateTime,
                expectedInstanceCount = 3,
                dicomInstances = efInstances)

type DicomSeriesRepository(context:EFModel.DicomDbContext) = 
    interface IAggregateRepository<DomainModel.DicomSeries, DomainModel.DicomUid> with
        member this.GetAggregateForKey(arg1: DomainModel.DicomUid): DomainModel.DicomSeries = 
            context.DicomSeries
            |> Seq.find (fun series -> 
                series.SeriesInstanceUid = arg1.UidString)
            |> mapSeriesEfToDomainModel

        member this.UpdateAsync(arg1: DomainModel.DicomSeries): Task = 
            raise (System.NotImplementedException())

