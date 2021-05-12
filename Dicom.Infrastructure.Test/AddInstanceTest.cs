using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using DomainModel = Dicom.Domain.Model;

namespace Dicom.Infrastructure.Test
{
    [TestClass]
    public class AddInstanceTest
    {
        string connectionString =
            @"Data Source=(localdb)\ProjectsV13;Initial Catalog=MyStoreDB;";

        [TestInitialize]
        public void SetupTest()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand deleteAttributes = new SqlCommand("delete from DicomAttributes", connection);
                deleteAttributes.ExecuteNonQuery();

                SqlCommand deleteInstances = new SqlCommand("delete from DicomInstances", connection);
                deleteInstances.ExecuteNonQuery();

                SqlCommand deleteSeries = new SqlCommand("delete from DicomSeries", connection);
                deleteSeries.ExecuteNonQuery();
            }
        }

        [TestMethod]
        public void TestCreatingSeries()
        {
            // construct a new series object and store the UID
            var newSeriesDomainModel = TestData.CreateSeries();
            var newSeriesUid = newSeriesDomainModel.SeriesInstanceUid;

            // NOTE that the database needs to be clean to run the test
            using (var context = new EFModel.MyContext())
            using (var repository = new Repositories.DicomSeriesRepository(context))
            {
                // perform an update to save it
                repository.UpdateAsync(newSeriesDomainModel).Wait();
            }

            // check that the new series exists
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string queryString = "select PatientId, Modality, AcquisitionDateTime "
                    + $"from DicomSeries where SeriesInstanceUID = '{newSeriesUid.ToString()}'";
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

        [TestMethod]
        public void TestAddingInstances()
        {
            // construct a new series object and store the UID
            var newSeriesDomainModel = TestData.CreateSeries();
            var newSeriesUid = newSeriesDomainModel.SeriesInstanceUid;

            // NOTE that the database needs to be clean to run the test
            using (var context = new EFModel.MyContext())
            using (var repository = new Repositories.DicomSeriesRepository(context))
            {
                // perform an update to save it
                repository.UpdateAsync(newSeriesDomainModel).Wait();

                // check that the new series exists
            }


            DomainModel.DicomSeries updateSeriesDomainModel = null;

            // now generate a new context / repository
            using (var context = new EFModel.MyContext())
            using (var repository = new Repositories.DicomSeriesRepository(context))
            {
                // now retreive the series domain model from the repository
                updateSeriesDomainModel = repository.GetAggregateForKey(newSeriesUid);

                // and instances to the series
                TestData.AddInstancesToSeries(updateSeriesDomainModel);

                // and update the result
                repository.UpdateAsync(updateSeriesDomainModel).Wait();
            }

            // check that the updated data is present
            // check that the new series exists
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string queryString = "select SopInstanceUid "
                    + "from DicomInstances inner join DicomSeries on DicomSeries.ID = DicomInstances.DicomSeriesId "
                    + $"where DicomSeries.SeriesInstanceUID = '{newSeriesUid.ToString()}'";

                SqlCommand command = new SqlCommand(queryString, connection);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    int rowCount = 0;

                    while (reader.Read())
                    {
                        rowCount++;

                        var sopInstanceUid = reader["SopInstanceUid"].ToString();

                        // there should be a single instance that matches the SOP instance UID
                        var matchInstances =
                            updateSeriesDomainModel.DicomInstances.Where(instance => 
                                instance.SopInstanceUid.ToString().CompareTo(sopInstanceUid) == 0)
                                .ToList();
                        Assert.AreEqual(matchInstances.Count, 1);
                    }

                    // should only have returned one row
                    Assert.AreEqual(rowCount, 3);
                }
            }
        }
    }
}
