using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using DomainModel = Elektrum.Capability.Dicom.Domain.Model;
using AbstractionModel = Elektrum.Capability.Dicom.Abstractions.Models;
using Elektrum.Capability.Dicom.Application.Helpers;
using Elektrum.Capability.Dicom.Application.Repositories;
using Elektrum.Capability.Dicom.Application.Services;
using Elektrum.Capability.Dicom.Api.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Elektrum.Capability.Dicom.Api.Test
{
    [TestClass]
    public class DicomControllerTest
    {
        /// <summary>
        /// test retrieve all series for a given patient ID
        /// </summary>
        [TestMethod]
        public void TestGetAllSeriesForPatient()
        {
            // common patient and series values
            var testPatientName = "Last, First";
            var testPatientId = "98765";
            var testAcquistionDateTime = DateTime.Now;

            // create a set of series to be returned
            var testDicomSerieses =
                Enumerable.Range(1, 5).Select(n =>
                        new DomainModel.DicomSeries(
                            new DomainModel.DicomUid($"1.2.3.{n}"),
                            testPatientName, testPatientId,
                            DomainModel.Modality.CT, testAcquistionDateTime, 3, null));

            // set up a mock service to return the serieses
            var mockService = new Mock<IDicomApplicationService>();
            mockService.Setup(svc =>
                svc.GetAllSeriesForPatient(It.Is<string>(s =>
                    s.Equals(testPatientId))))
                .Returns(testDicomSerieses);

            // hook up a controller to call in to
            DicomController testController = CreateTestController(mockService.Object);

            // now call the controller for the test patient
            var okObjectResult = (OkObjectResult)testController.GetAllDicomSeriesForPatient(testPatientId).Result;

            // and get the list of series that was returned
            var listDicomSeries = (IEnumerable<AbstractionModel.DicomSeries>)okObjectResult.Value;

            // make sure the proper number are returned
            Assert.AreEqual(5, listDicomSeries.Count());

            // for each, check that the properties match what was provded
            foreach (var dicomSeries in listDicomSeries)
            {
                Assert.AreEqual(testPatientId, dicomSeries.PatientId);
                Assert.AreEqual(testPatientName, dicomSeries.PatientName);
                Assert.AreEqual(DomainModel.Modality.CT.ToString(), dicomSeries.Modality);
                Assert.AreEqual(testAcquistionDateTime, dicomSeries.AcquisitionDateTime);
            }

            // make sure the SeriesInstanceUids for each is distinct
            var allDistinctSeriesInstanceUids =
                listDicomSeries.Select(dicomSeries => dicomSeries.SeriesInstanceUid).Distinct();
            Assert.AreEqual(5, allDistinctSeriesInstanceUids.Count());
        }

        /// <summary>
        /// test getting a series by series instance UID
        /// </summary>
        [TestMethod]
        public void TestGetDicomSeries()
        {
            // set up the test data to be retrieved
            var testPatientName = "Last, First";
            var testPatientId = "98765";
            var testAcquistionDateTime = DateTime.Now;

            var testSeriesInstanceUid = new DomainModel.DicomUid("1.2.3.7");

            var mockDicomSeries =
                new DomainModel.DicomSeries(testSeriesInstanceUid, 
                    testPatientName, testPatientId, DomainModel.Modality.CT, testAcquistionDateTime, 3, null);

            // hook up a mock repository to return the mockDicomSeries
            var mockRepository = new Mock<IAggregateRepository<DomainModel.DicomSeries, DomainModel.DicomUid>>();
            mockRepository.Setup(repo =>
                    repo.GetAggregateForKey(It.Is<DomainModel.DicomUid>(s =>
                        s.Equals(testSeriesInstanceUid))))
                .Returns(mockDicomSeries);

            // and create the service and controller to be tested
            var dicomParser = new DicomParser();
            var messaging = new Messaging.Messaging(new NullLogger<Messaging.Messaging>());
            var service = new DicomApplicationService(mockRepository.Object, messaging, dicomParser, new NullLogger<DicomApplicationService>());

            var testController = CreateTestController(service);

            var okObjectResult = (OkObjectResult)testController.GetDicomSeries(testPatientId, testSeriesInstanceUid.ToString()).Result;
            var abDicomSeries = (AbstractionModel.DicomSeries)okObjectResult.Value;

            Assert.AreEqual(testSeriesInstanceUid.ToString(), abDicomSeries.SeriesInstanceUid);
            Assert.AreEqual(testPatientName, abDicomSeries.PatientName);
            Assert.AreEqual(testPatientId, abDicomSeries.PatientId);
            Assert.AreEqual(DomainModel.Modality.CT.ToString(), abDicomSeries.Modality);
            Assert.AreEqual(testAcquistionDateTime, abDicomSeries.AcquisitionDateTime);
            Assert.AreEqual(3, abDicomSeries.ExpectedInstanceCount);
        }

        /// <summary>
        /// testing adding a dicom series for given metadata
        /// </summary>
        [TestMethod]
        public void TestAddDicomSeries()
        {
            // create a series abstraction record to add
            var testAbDicomSeries =
                new AbstractionModel.DicomSeries()
                {
                    PatientId = "98754",
                    PatientName = "Last, First",
                    SeriesInstanceUid = "1.2.3.7",
                    Modality = "CT",
                    ExpectedInstanceCount = 1,
                    AcquisitionDateTime = DateTime.Now,
                };

            var mockRepository = new Mock<IAggregateRepository<DomainModel.DicomSeries, DomainModel.DicomUid>>();
            mockRepository.Setup(repo =>
                repo.UpdateAsync(It.IsAny<DomainModel.DicomSeries>()))
                .Returns(Task.CompletedTask)
                .Callback((DomainModel.DicomSeries s) =>
                {
                    Assert.AreEqual(testAbDicomSeries.PatientId, s.PatientId);
                    Assert.AreEqual(testAbDicomSeries.PatientName, s.PatientName);
                    Assert.AreEqual(testAbDicomSeries.SeriesInstanceUid, s.SeriesInstanceUid.ToString());
                    Assert.AreEqual(testAbDicomSeries.AcquisitionDateTime, s.AcquisitionDateTime);
                    Assert.AreEqual(testAbDicomSeries.Modality, s.Modality.ToString());
                    Assert.AreEqual(testAbDicomSeries.AcquisitionDateTime, s.AcquisitionDateTime);
                    Assert.AreEqual(DomainModel.SeriesState.Created, s.CurrentState);
                });

            var dicomParser = new DicomParser();
            var messaging = new Messaging.Messaging(new NullLogger<Messaging.Messaging>());
            var service = new DicomApplicationService(mockRepository.Object, messaging, dicomParser, new NullLogger<DicomApplicationService>());

            var testController = CreateTestController(service);

            // and try the operation
            testController.AddDicomSeries(testAbDicomSeries.PatientId, testAbDicomSeries).Wait();
        }

        /// <summary>
        /// test deleting a dicom series by UID
        /// </summary>
        [TestMethod]
        public void TestDeleteDicomSeries()
        {
            var testSeriesInstanceUid = new DomainModel.DicomUid("1.2.3.7");

            var mockRepository = new Mock<IAggregateRepository<DomainModel.DicomSeries, DomainModel.DicomUid>>();
            mockRepository.Setup(repo =>
                    repo.RemoveAsync(It.Is<DomainModel.DicomUid>(uid => uid.Equals(testSeriesInstanceUid))))
                .Returns(Task.CompletedTask);

            // and create the service and controller to be tested
            var dicomParser = new DicomParser();
            var messaging = new Messaging.Messaging(new NullLogger<Messaging.Messaging>());
            var service = new DicomApplicationService(mockRepository.Object, messaging, dicomParser, new NullLogger<DicomApplicationService>());

            var testController = CreateTestController(service);

            testController.DeleteDicomSeries(testSeriesInstanceUid.ToString()).Wait();

            // that's it
        }

        /// <summary>
        /// test adding a dicom instance from a blob
        /// </summary>
        [TestMethod]
        [Ignore]  //ignoring because the test data isn't in the correct place
        public void TestAddDicomInstance()
        {
            var dicomInstanceStream = new FileStream(@"TestData\DXIMAGEA.dcm", FileMode.Open);
            var dicomFormFile = new FormFile(dicomInstanceStream, 0, 0, "DICOM", "DXIMAGEA.dcm");

            var patientName = "Test^PixelSpacing";
            var patientId = "62354PQGRRST";
            var seriesInstanceUid = new DomainModel.DicomUid("1.3.6.1.4.1.5962.1.1.65535.103.1.1239106253.3783.0");

            var mockSeries = new DomainModel.DicomSeries(seriesInstanceUid, patientName, patientId, DomainModel.Modality.DX, DateTime.Now, 1);
            var mockRepository = new Mock<IAggregateRepository<DomainModel.DicomSeries, DomainModel.DicomUid>>();
            mockRepository.Setup(repo =>
                    repo.GetAggregateForKey(It.Is<DomainModel.DicomUid>(uid => uid.Equals(seriesInstanceUid))))
                .Returns(mockSeries);
            mockRepository.Setup(repo =>
                    repo.UpdateAsync(It.IsAny<DomainModel.DicomSeries>()))
                .Returns(Task.CompletedTask)
                .Callback((DomainModel.DicomSeries series) =>
                {
                    Assert.AreEqual(1, series.DicomInstances.Count());

                    var newInstance = series.DicomInstances.First();

                    CheckAttribute(newInstance, DomainModel.DicomTag.PATIENTNAME, patientName);
                    CheckAttribute(newInstance, DomainModel.DicomTag.PATIENTID, patientId);
                    CheckAttribute(newInstance, DomainModel.DicomTag.MODALITY, "DX");
                });

            // now set up the service with the mock and a dicom parser
            var dicomParser = new DicomParser();
            var messaging = new Messaging.Messaging(new NullLogger<Messaging.Messaging>());
            var service = new DicomApplicationService(mockRepository.Object, messaging, dicomParser, new NullLogger<DicomApplicationService>());

            // and set up the test controller
            var testController = CreateTestController(service);

            // call to add the dicom instance
            testController.AddDicomInstance("1.3.6.1.4.1.5962.1.1.65535.103.1.1239106253.3783.0", dicomFormFile).Wait();

            System.Diagnostics.Trace.WriteLine("Done adding new instance");
        }

        /// <summary>
        /// test for getting a dicom instance
        /// </summary>
        [TestMethod]
        public void TestGetDicomInstance()
        {
            var patientName = "Last^First";
            var patientId = "987321";
            var seriesInstanceUid = new DomainModel.DicomUid("1.3.9.5");
            var sopInstanceUid = new DomainModel.DicomUid("1.3.9.7");
            var mockInstance = 
                new DomainModel.DicomInstance(sopInstanceUid,
                    new List<DomainModel.DicomAttribute>()
                    {
                        new DomainModel.DicomAttribute(DomainModel.DicomTag.PATIENTID, patientId),
                        new DomainModel.DicomAttribute(DomainModel.DicomTag.PATIENTNAME, patientName),
                        new DomainModel.DicomAttribute(DomainModel.DicomTag.SERIESINSTANCEUID, seriesInstanceUid.ToString()),
                        new DomainModel.DicomAttribute(DomainModel.DicomTag.SOPINSTANCEUID, sopInstanceUid.ToString()),
                        new DomainModel.DicomAttribute(DomainModel.DicomTag.MODALITY, "CT"),
                    });
            var mockSeries = 
                new DomainModel.DicomSeries(seriesInstanceUid, 
                    patientName, patientId, 
                    DomainModel.Modality.DX, DateTime.Now, 1, 
                    new List<DomainModel.DicomInstance>()
                    {
                        mockInstance
                    });

            var mockRepository = new Mock<IAggregateRepository<DomainModel.DicomSeries, DomainModel.DicomUid>>();
            mockRepository.Setup(repo =>
                    repo.GetAggregateForKey(It.Is<DomainModel.DicomUid>(uid => uid.Equals(seriesInstanceUid))))
                .Returns(mockSeries);

            // now set up the service with the mock and a dicom parser
            var dicomParser = new DicomParser();
            var messaging = new Messaging.Messaging(new NullLogger<Messaging.Messaging>());
            var service = new DicomApplicationService(mockRepository.Object, messaging, dicomParser, new NullLogger<DicomApplicationService>());

            var httpContext = new DefaultHttpContext();

            // and set up the test controller
            var testController = CreateTestController(service);
            testController.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };

            var okObjectResult = (OkObjectResult)testController.GetDicomInstance(seriesInstanceUid.ToString(), sopInstanceUid.ToString()).Result;
            var retrievedDicomInstance = (AbstractionModel.DicomInstance)okObjectResult.Value;

            Assert.AreEqual(sopInstanceUid.ToString(), retrievedDicomInstance.SopInstanceUid);

            var retrievedPatientId = 
                retrievedDicomInstance.DicomAttributes.Single(element => 
                    element.DicomTag.Equals(DomainModel.DicomTag.PATIENTID.ToString()));
            Assert.AreEqual(patientId, retrievedPatientId.Value);

            var retrievedPatientName =
                retrievedDicomInstance.DicomAttributes.Single(element => 
                    element.DicomTag.Equals(DomainModel.DicomTag.PATIENTNAME.ToString()));

            var retrievedModality =
                retrievedDicomInstance.DicomAttributes.Single(element =>
                    element.DicomTag.Equals(DomainModel.DicomTag.MODALITY.ToString()));
            Assert.AreEqual("CT", retrievedModality.Value);
        }

        /// <summary>
        /// test for getting a dicom instance using a filter query
        /// </summary>
        [TestMethod]
        public void TestGetDicomInstanceWithFilterQuery()
        {
            var patientName = "Last^First";
            var patientId = "987321";
            var seriesInstanceUid = new DomainModel.DicomUid("1.3.9.5");
            var sopInstanceUid = new DomainModel.DicomUid("1.3.9.7");
            var mockInstance =
                new DomainModel.DicomInstance(sopInstanceUid,
                    new List<DomainModel.DicomAttribute>()
                    {
                        new DomainModel.DicomAttribute(DomainModel.DicomTag.PATIENTID, patientId),
                        new DomainModel.DicomAttribute(DomainModel.DicomTag.PATIENTNAME, patientName),
                        new DomainModel.DicomAttribute(DomainModel.DicomTag.SERIESINSTANCEUID, seriesInstanceUid.ToString()),
                        new DomainModel.DicomAttribute(DomainModel.DicomTag.SOPINSTANCEUID, sopInstanceUid.ToString()),
                        new DomainModel.DicomAttribute(DomainModel.DicomTag.MODALITY, "CT"),
                    });
            var mockSeries =
                new DomainModel.DicomSeries(seriesInstanceUid,
                    patientName, patientId,
                    DomainModel.Modality.DX, DateTime.Now, 1,
                    new List<DomainModel.DicomInstance>()
                    {
                        mockInstance
                    });

            var mockRepository = new Mock<IAggregateRepository<DomainModel.DicomSeries, DomainModel.DicomUid>>();
            mockRepository.Setup(repo =>
                    repo.GetAggregateForKey(It.Is<DomainModel.DicomUid>(uid => uid.Equals(seriesInstanceUid))))
                .Returns(mockSeries);

            // now set up the service with the mock and a dicom parser
            var dicomParser = new DicomParser();
            var messaging = new Messaging.Messaging(new NullLogger<Messaging.Messaging>());
            var service = new DicomApplicationService(mockRepository.Object, messaging, dicomParser, new NullLogger<DicomApplicationService>());

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Query =
                new QueryCollection(
                    new Dictionary<string, StringValues>()
                    {
                        { "query",
                            new StringValues(
                                String.Join('+',
                                    new string[]
                                    {
                                        DomainModel.DicomTag.PATIENTID.ToString(),
                                        DomainModel.DicomTag.PATIENTNAME.ToString(),
                                        DomainModel.DicomTag.SOPINSTANCEUID.ToString()
                                    })) },
                    });

            // and set up the test controller
            var testController = CreateTestController(service);
            testController.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };

            var okObjectResult = (OkObjectResult)testController.GetDicomInstance(seriesInstanceUid.ToString(), sopInstanceUid.ToString()).Result;
            var retrievedDicomInstance = (AbstractionModel.DicomInstance)okObjectResult.Value;

            Assert.AreEqual(sopInstanceUid.ToString(), retrievedDicomInstance.SopInstanceUid);
            Assert.AreEqual(3, retrievedDicomInstance.DicomAttributes.Count());

            var retrievedPatientId =
                retrievedDicomInstance.DicomAttributes.Single(element =>
                    element.DicomTag.Equals(DomainModel.DicomTag.PATIENTID.ToString()));
            Assert.AreEqual(patientId, retrievedPatientId.Value);

            var retrievedPatientName =
                retrievedDicomInstance.DicomAttributes.Single(element =>
                    element.DicomTag.Equals(DomainModel.DicomTag.PATIENTNAME.ToString()));
            Assert.AreEqual(patientId, retrievedPatientId.Value);
        }

        private static DicomController CreateTestController(IDicomApplicationService service)
        {
            var config = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.AddProfile<Mappers.AbstractionMapperProfile>();
            });
            var mapper = config.CreateMapper();
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<DicomController>();
            return new DicomController(service, mapper, logger);
        }

        private void CheckAttribute(DomainModel.DicomInstance newInstance, DomainModel.DicomTag forTag, string expectedValue)
        {
            var attribute =
                newInstance.DicomAttributes.Single(element => element.DicomTag.Equals(forTag));
            Assert.AreEqual(expectedValue, attribute.Value);
        }
    }
}
