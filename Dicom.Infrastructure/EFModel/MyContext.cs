using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dicom.Infrastructure.EFModel
{
    public class MyContext : DbContext
    {
        private readonly ILogger<MyContext> _logger;

        public MyContext()
        {
        }

        public MyContext(DbContextOptions<MyContext> options, ILogger<MyContext> logger)
            : base(options)
        {
            _logger = logger;
        }

        public DbSet<DicomSeries> DicomSeries { get; set; }
        public DbSet<DicomInstance> DicomInstances { get; set; }
        public DbSet<DicomAttribute> DicomAttributes { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\ProjectsV13;Initial Catalog=MyStoreDB;");
        }
    }
}
