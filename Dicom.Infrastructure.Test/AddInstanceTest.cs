using Microsoft.Data.SqlClient;
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

                string connectionString = 
                    @"Data Source=(localdb)\ProjectsV13;Initial Catalog=MyStoreDB;";

                // check that the new series exists
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string queryString = $"select PatientId, Modality, AcquisitionDateTime from DicomSeries where SeriesInstanceUID = '{newSeriesUid.ToString()}'";
                    SqlCommand command = new SqlCommand(queryString, connection);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int rowCount = 0;

                        while (reader.Read())
                        {
                            rowCount++;

                            var patientId = reader["PatientId"].ToString();
                            Assert.AreEqual(newSeriesDomainModel.PatientId, patientId);

                            var modality = reader["Modality"].ToString();
                            Assert.AreEqual(newSeriesDomainModel.Modality, modality);

                            var acquisitionDateTime = System.DateTime.Parse(reader["AcquisitionDateTime"].ToString());
                            Assert.AreEqual(newSeriesDomainModel.AcquisitionDateTime, acquisitionDateTime);
                        }

                        // should only have returned one row
                        Assert.AreEqual(rowCount, 1);
                    }
                }
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
