using Microsoft.EntityFrameworkCore;

namespace TestDicomDomainMapper.EFModel
{
    public class MyContext : DbContext
    {
        public DbSet<DicomSeries> DicomSeries { get; set; }
        public DbSet<DicomInstance> DicomInstances { get; set; }
        public DbSet<DicomAttribute> DicomAttributes { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\ProjectsV13;Initial Catalog=MyStoreDB;");
        }
    }
}
