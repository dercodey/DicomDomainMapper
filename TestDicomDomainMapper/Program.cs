using DomainModel = Elektrum.Capability.Dicom.Domain.Model;
using EFModel = Elektrum.Capability.Dicom.Infrastructure.EFModel;
using Repositories = Elektrum.Capability.Dicom.Infrastructure.Repositories;

namespace TestDicomDomainMapper
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. try just creating an EF model
            // TestData.CreatePopulateEFModel();

            // 2.
            DomainModel.DicomUid newSeriesUid = null;
            {
                var context = new EFModel.DicomDbContext();
                var repository = new Repositories.DicomSeriesRepository(context);

                var newSeriesDomainModel = TestData.CreateSeries();

                repository.UpdateAsync(newSeriesDomainModel).Wait();

                // capture new series ID
                newSeriesUid = newSeriesDomainModel.SeriesInstanceUid;
            }

            // 3.
            {
                var context = new EFModel.DicomDbContext();
                var repository = new Repositories.DicomSeriesRepository(context);

                var updateSeriesDomainModel = repository.GetAggregateForKey(newSeriesUid);

                TestData.AddInstancesToSeries(updateSeriesDomainModel);

                repository.UpdateAsync(updateSeriesDomainModel).Wait();
            }
        }
    }
}
