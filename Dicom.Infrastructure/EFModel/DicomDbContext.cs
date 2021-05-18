using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Elekta.Capability.Dicom.Infrastructure.EFModel
{
    public class DicomDbContext : DbContext
    {
        private readonly ILogger<DicomDbContext> _logger;

        public DicomDbContext()
        {
        }

        public DicomDbContext(DbContextOptions<DicomDbContext> options, ILogger<DicomDbContext> logger)
            : base(options)
        {
            _logger = logger;
        }

        public DbSet<DicomSeries> DicomSeries { get; set; }
        public DbSet<DicomInstance> DicomInstances { get; set; }
        public DbSet<DicomElement> DicomElements { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\ProjectsV13;Initial Catalog=MyStoreDB;");
        }
    }
}
