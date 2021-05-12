using Microsoft.VisualStudio.TestTools.UnitTesting;
using DomainModel = Dicom.Domain.Model;

namespace Dicom.Infrastructure.Test
{
    [TestClass]
    public class AddInstanceTest
    {
        [TestInitialize]
        public void SetupTest()
        {
        }

        [TestMethod]
        public void TestCreatingSeries()
        {
            // NOTE that the database needs to be clean to run the test
            using (var context = new EFModel.MyContext())
            using (var repository = new Repositories.DicomSeriesRepository(context))
            {
                // construct a new series object and store the UID
                var newSeriesDomainModel = TestData.CreateSeries();
                var newSeriesUid = newSeriesDomainModel.SeriesInstanceUid;

                // perform an update to save it
                repository.UpdateAsync(newSeriesDomainModel).Wait();

                // check that the new series exists
            }
        }

        [TestMethod]
        public void TestAddingInstances()
        {
            DomainModel.DicomUid newSeriesUid = null;

            // NOTE that the database needs to be clean to run the test
            using (var context = new EFModel.MyContext())
            using (var repository = new Repositories.DicomSeriesRepository(context))
            {
                // construct a new series object and store the UID
                var newSeriesDomainModel = TestData.CreateSeries();
                newSeriesUid = newSeriesDomainModel.SeriesInstanceUid;

                // perform an update to save it
                repository.UpdateAsync(newSeriesDomainModel).Wait();

                // check that the new series exists
            }


            // now generate a new context / repository
            using (var context = new EFModel.MyContext())
            using (var repository = new Repositories.DicomSeriesRepository(context))
            {
                // now retreive the series domain model from the repository
                var updateSeriesDomainModel = repository.GetAggregateForKey(newSeriesUid);

                // and instances to the series
                TestData.AddInstancesToSeries(updateSeriesDomainModel);

                // and update the result
                repository.UpdateAsync(updateSeriesDomainModel).Wait();

                // check that the updated data is present

            }
        }
    }
}
