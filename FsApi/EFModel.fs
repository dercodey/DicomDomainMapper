module EFModel

open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Logging

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