using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Dicom.Application.Repositories;
using Dicom.Application.Services;
using Dicom.Application.Helpers;
using DomainModel = Dicom.Domain.Model;
using Infrastructure = Dicom.Infrastructure;

namespace Elekta.Capability.Dicom.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<Infrastructure.EFModel.DicomDbContext>(options =>
            {
                options.UseSqlServer(@"Data Source=(localdb)\ProjectsV13;Initial Catalog=MyStoreDB;");
            },
            ServiceLifetime.Scoped);

            services.AddScoped<IAggregateRepository<DomainModel.DicomSeries, DomainModel.DicomUid>,
                Infrastructure.Repositories.DicomSeriesRepository>();
            services.AddScoped<IDicomParser, DicomParser>();
            services.AddScoped<IDicomApplicationService, DicomApplicationService>();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
