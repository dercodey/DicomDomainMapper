namespace FsDomain

open Microsoft.EntityFrameworkCore
open System.Threading.Tasks
open Microsoft.Extensions.Logging

module Seedwork =
    type IAggregateRoot<'key> =
        abstract RootKey : 'key

    type IEntity<'key> =
        abstract Key : 'key

module DomainModel =

    [<CLIMutable>]
    type DicomTag = {
        Group:int;
        Element:int; 
    }

    [<CLIMutable>]
    type DicomUid = {
        UidString:string
    }

    [<CLIMutable>]
    type DicomAttribute = {
        DicomTag:DicomTag;
        Value:string;
    }

    type DicomInstance(sopInstanceUid:DicomUid, dicomAttributes:seq<DicomAttribute>) =
        // check that attributes are OK
        member val SopInstanceUid = sopInstanceUid
        member val DicomAttributes = dicomAttributes

        interface Seedwork.IEntity<DicomUid> with
            member val Key : DicomUid = Unchecked.defaultof<DicomUid>

    type Modality =
    | CT
    | DX

    type DicomSeries(seriesInstanceUid:DicomUid, 
            patientName:string, patientId:string, 
            modality:Modality, 
            acquisitionDateTime:System.DateTime, 
            expectedInstanceCount:int,
            dicomInstances:seq<DicomInstance>) =

        // check that attributes are OK
        let mutable dicomInstances : list<DicomInstance> = [ ]
        member val SeriesInstanceUid = seriesInstanceUid
        member val DicomInstances = dicomInstances
        member this.AddInstance (dicomInstance:DicomInstance) =
            let seriesFromInstance = dicomInstance.DicomAttributes |> Seq.head
            if (seriesFromInstance.Value = seriesInstanceUid.ToString())
            then 
                dicomInstances <- dicomInstance :: dicomInstances
                dicomInstances |> List.length |> Result.Ok
            else
                "seriesInstanceUid mismatch" |> Result.Error

        interface Seedwork.IAggregateRoot<DicomUid> with
            member val RootKey : DicomUid = Unchecked.defaultof<DicomUid>


module EFModel =
    [<CLIMutable>]
    type DicomAttribute = {
        ID:int;
        DicomTag:string;
        Value:string;
        DicomInstanceId:int;
    }

    [<CLIMutable>]
    type DicomInstance = {
        ID:int;
        SopInstanceUid:string;
        DicomSeriesId:int;
        DicomAttributes:seq<DicomAttribute>;
    }

    [<CLIMutable>]
    type DicomSeries = {
        ID:int;
        PatientId:string;
        PatientName:string;
        SeriesInstanceUid:string;
        Modality:string;
        AcquistionDateTime:System.DateTime;
        DicomInstances:seq<DicomInstance>;
    }

    type DicomDbContext(options:DbContextOptions<DicomDbContext>, logger:ILogger<DicomDbContext>) =
        inherit DbContext()
        override this.OnConfiguring(optionsBuilder:DbContextOptionsBuilder) =
            @"Data Source=(localdb)\ProjectsV13;Initial Catalog=MyStoreDB;"
            |> optionsBuilder.UseSqlServer
            |> ignore

        member val DicomSeries : DbSet<DicomSeries> = null with get, set
        member val DicomInstances : DbSet<DicomInstance> = null with get, set
        member val DicomAttributes : DbSet<DicomAttribute> = null with get, set


module Repository = 
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

module Application =
    type IDicomApplicationService =         
        abstract member GetAllSeriesForPatient : string -> seq<DomainModel.DicomSeries> 
        abstract member CreateSeriesAsync : DomainModel.DicomSeries -> Task

    type DicomApplicationService(repository:Repository.IAggregateRepository<DomainModel.DicomSeries, DomainModel.DicomUid>) = 
        interface IDicomApplicationService with
            member this.CreateSeriesAsync(arg1: DomainModel.DicomSeries): Task = 
                raise (System.NotImplementedException())
            member this.GetAllSeriesForPatient(arg1: string): seq<DomainModel.DicomSeries> = 
                raise (System.NotImplementedException())