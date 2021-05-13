using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using DomainModel = Dicom.Domain.Model;

namespace Dicom.Infrastructure.Test
{
    [TestClass]
    public class AddInstanceTest
    {
        private string connectionString =
            @"Data Source=(localdb)\ProjectsV13;Initial Catalog=MyStoreDB;";

        private string[] tableNames = 
            new string[] 
            { 
                "DicomAttributes", 
                "DicomInstances", 
                "DicomSeries" 
            };

        [TestInitialize]
        public void SetupTest()
        {            
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                foreach (var tableName in tableNames)
                {
                    // delete any existing rows from the tables
                    var deleteCommand = new SqlCommand($"delete from {tableName}", connection);
                    deleteCommand.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// testing creating a new series
        /// </summary>
        [TestMethod]
        public void TestCreatingSeries()
        {
            // construct a new series object and store the UID
            var newSeriesDomainModel = TestData.CreateSeries();
            var newSeriesUid = newSeriesDomainModel.SeriesInstanceUid;

            var mapper = Mappers.MyMapper.GetMapper();

            // NOTE that the database needs to be clean to run the test
            using (var context = new EFModel.MyContext())
            using (var repository = new Repositories.DicomSeriesRepository(context, mapper))
            {
                // perform an update to save it
                repository.UpdateAsync(newSeriesDomainModel).Wait();
            }

            var queryString = "select PatientId, Modality, AcquisitionDateTime "
                + $"from DicomSeries where SeriesInstanceUID = '{newSeriesUid.ToString()}'";

            int rowCount = QueryAndTest(queryString, reader =>
            {
                var patientId = reader["PatientId"].ToString();
                Assert.AreEqual(newSeriesDomainModel.PatientId, patientId);

                var modality = reader["Modality"].ToString();
                Assert.AreEqual(newSeriesDomainModel.Modality, modality);

                var acquisitionDateTime = System.DateTime.Parse(reader["AcquisitionDateTime"].ToString());
                Assert.AreEqual(newSeriesDomainModel.AcquisitionDateTime, acquisitionDateTime);
            });

            // should only have returned one row
            Assert.AreEqual(rowCount, 1);
        }

        /// <summary>
        /// test creating a series and adding instances to the series
        /// </summary>
        [TestMethod]
        public void TestAddingInstances()
        {
            // construct a new series object and store the UID
            var newSeriesDomainModel = TestData.CreateSeries();
            var newSeriesUid = newSeriesDomainModel.SeriesInstanceUid;

            var mapper = Mappers.MyMapper.GetMapper();

            // NOTE that the database needs to be clean to run the test
            using (var context = new EFModel.MyContext())
            using (var repository = new Repositories.DicomSeriesRepository(context, mapper))
            {
                // perform an update to save it
                repository.UpdateAsync(newSeriesDomainModel).Wait();

                // check that the new series exists
            }

            DomainModel.DicomSeries updateSeriesDomainModel = null;

            // now generate a new context / repository
            using (var context = new EFModel.MyContext())
            using (var repository = new Repositories.DicomSeriesRepository(context, mapper))
            {
                // now retreive the series domain model from the repository
                updateSeriesDomainModel = repository.GetAggregateForKey(newSeriesUid);

                // and instances to the series
                TestData.AddInstancesToSeries(updateSeriesDomainModel);

                // and update the result
                repository.UpdateAsync(updateSeriesDomainModel).Wait();
            }

            List<string> sopInstanceUids = new List<string>();

            // check that the updated data is present
            var queryString = "select SopInstanceUid "
                + "from DicomInstances inner join DicomSeries on DicomSeries.ID = DicomInstances.DicomSeriesId "
                + $"where DicomSeries.SeriesInstanceUID = '{newSeriesUid.ToString()}'";
            QueryAndTest(queryString, reader =>
            {
                var sopInstanceUid = reader["SopInstanceUid"].ToString();

                // there should be a single instance that matches the SOP instance UID
                var matchInstances =
                    updateSeriesDomainModel.DicomInstances.Where(instance =>
                        instance.SopInstanceUid.ToString().CompareTo(sopInstanceUid) == 0)
                        .ToList();
                Assert.AreEqual(matchInstances.Count, 1);
                Console.WriteLine($"Found instance = {sopInstanceUid}");

                sopInstanceUids.Add(matchInstances.Single().SopInstanceUid.ToString());
            });

            // should only have returned one row
            Assert.AreEqual(sopInstanceUids.Count, 3);

            foreach (var sopInstanceUid in sopInstanceUids)
            {
                var queryAttributeString = "select DicomTag "
                    + "from DicomAttributes inner join DicomInstances on DicomInstances.ID = DicomAttributes.DicomInstanceId "
                    + $"where DicomInstances.SopInstanceUid = '{sopInstanceUid}'";
                int attributeRowsFound = QueryAndTest(queryAttributeString, reader =>
                {
                    var dicomTag = reader["DicomTag"];
                    Console.WriteLine($"Found tag = {dicomTag}");
                });

                Assert.AreEqual(attributeRowsFound, 5);
            }
        }

        /// <summary>
        /// helper to perform a query and then call a test action on the resulting reader
        /// </summary>
        /// <param name="queryString">the query to be executed</param>
        /// <param name="testAction">action of SqlDataReader that will test each row</param>
        /// <returns>number of rows returned by the query</returns>
        int QueryAndTest(string queryString, Action<SqlDataReader> testAction)
        {
            int rowCount = 0;

            // check that the new series exists
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = new SqlCommand(queryString, connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rowCount++;

                        testAction(reader);
                    }
                }
            }

            return rowCount;
        }
    }
}
