namespace FsApi

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.EntityFrameworkCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.HttpsPolicy;
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open FsDomain.DomainModel
open FsDomain.EFModel
open FsDomain.Repository
open FsDomain.Application


type Startup private () =
    new (configuration: IConfiguration) as this =
        Startup() then
        this.Configuration <- configuration

    // This method gets called by the runtime. Use this method to add services to the container.
    member this.ConfigureServices(services: IServiceCollection) =
        services.AddDbContext<DicomDbContext>((fun (options:DbContextOptionsBuilder) -> 
            options.UseSqlServer(@"Data Source=(localdb)\ProjectsV13;Initial Catalog=MyStoreDB;") 
                |> ignore), 
            ServiceLifetime.Scoped) 
            |> ignore
        services.AddScoped<IAggregateRepository<FsDomain.DomainModel.DicomSeries, DicomUid>, 
                                DicomSeriesRepository>() |> ignore
        services.AddScoped<IDicomApplicationService, DicomApplicationService>() |> ignore
        services.AddControllers() |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        if (env.IsDevelopment()) then
            app.UseDeveloperExceptionPage() |> ignore

        app.UseHttpsRedirection() |> ignore
        app.UseRouting() |> ignore

        app.UseAuthorization() |> ignore

        app.UseEndpoints(fun endpoints ->
            endpoints.MapControllers() |> ignore
            ) |> ignore

    member val Configuration : IConfiguration = null with get, set
