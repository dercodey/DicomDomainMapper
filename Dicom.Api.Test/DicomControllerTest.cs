using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using DomainModel = Elekta.Capability.Dicom.Domain.Model;
using AbstractionModel = Elekta.Capability.Dicom.Abstractions.Models;
using Elekta.Capability.Dicom.Application.Helpers;
using Elekta.Capability.Dicom.Application.Repositories;
using Elekta.Capability.Dicom.Application.Services;
using Elekta.Capability.Dicom.Api.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Elekta.Capability.Dicom.Api.Test
{
    [TestClass]
    public class DicomControllerTest
    {
        [TestMethod]
        public void TestGetAllSeriesForPatient()
        {
            var testPatientName = "Last, First";
            var testPatientId = "98765";
            var testAcquistionDateTime = DateTime.Now;

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
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<DicomController>();
            var testController = new DicomController(mockService.Object, logger);

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
            var service = new DicomApplicationService(mockRepository.Object, dicomParser);

            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<DicomController>();
            var testController = new DicomController(service, logger);

            var okObjectResult = (OkObjectResult)testController.GetDicomSeries(testPatientId, testSeriesInstanceUid.ToString()).Result;
            var abDicomSeries = (AbstractionModel.DicomSeries)okObjectResult.Value;

            Assert.AreEqual(testSeriesInstanceUid.ToString(), abDicomSeries.SeriesInstanceUid);
            Assert.AreEqual(testPatientName, abDicomSeries.PatientName);
            Assert.AreEqual(testPatientId, abDicomSeries.PatientId);
            Assert.AreEqual(DomainModel.Modality.CT.ToString(), abDicomSeries.Modality);
            Assert.AreEqual(testAcquistionDateTime, abDicomSeries.AcquisitionDateTime);
            Assert.AreEqual(3, abDicomSeries.ExpectedInstanceCount);
        }


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

            // set up a mock service that will receive the new series
            var _mockService = new Mock<IDicomApplicationService>();
            _mockService.Setup(svc => 
                    svc.CreateSeriesAsync(It.IsAny<DomainModel.DicomSeries>()))
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

            // set up a controller to exercise add series
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<DicomController>();
            var testController = new DicomController(_mockService.Object, logger);

            // and try the operation
            testController.AddDicomSeries(testAbDicomSeries.PatientId, testAbDicomSeries).Wait();
        }

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
            var service = new DicomApplicationService(mockRepository.Object, dicomParser);

            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<DicomController>();
            var testController = new DicomController(service, logger);

            testController.DeleteDicomSeries(testSeriesInstanceUid.ToString()).Wait();

            // that's it
        }

        [TestMethod]
        public void TestAddDicomInstance()
        {
            var dicomInstanceStream = new FileStream(@"TestData\DXIMAGEA.dcm", FileMode.Open);

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

                    // check elements
                    var addedPatientName =
                        newInstance.DicomElements.Single(element => element.DicomTag.Equals(DomainModel.DicomTag.PATIENTNAME));
                    Assert.AreEqual(patientName, addedPatientName.Value);

                    var addedPatientId =
                        newInstance.DicomElements.Single(element => element.DicomTag.Equals(DomainModel.DicomTag.PATIENTID));
                    Assert.AreEqual(patientId, addedPatientId.Value);

                    var addedModality =
                        newInstance.DicomElements.Single(element => element.DicomTag.Equals(DomainModel.DicomTag.MODALITY));
                    Assert.AreEqual("DX", addedModality.Value);
                });

            // now set up the service with the mock and a dicom parser
            var dicomParser = new DicomParser();
            var service = new DicomApplicationService(mockRepository.Object, dicomParser);

            // and set up the test controller
            var testController = new DicomController(service, new NullLogger<DicomController>());

            // call to add the dicom instance
            testController.AddDicomInstance("1.3.6.1.4.1.5962.1.1.65535.103.1.1239106253.3783.0", dicomInstanceStream).Wait();

            System.Diagnostics.Trace.WriteLine("Done adding new instance");
        }

        [TestMethod]
        public void TestGetDicomInstance()
        {
            var patientName = "Last^First";
            var patientId = "987321";
            var seriesInstanceUid = new DomainModel.DicomUid("1.3.9.5");
            var sopInstanceUid = new DomainModel.DicomUid("1.3.9.7");
            var mockInstance = 
                new DomainModel.DicomInstance(sopInstanceUid,
                    new List<DomainModel.DicomElement>()
                    {
                        new DomainModel.DicomElement(DomainModel.DicomTag.PATIENTID, patientId),
                        new DomainModel.DicomElement(DomainModel.DicomTag.PATIENTNAME, patientName),
                        new DomainModel.DicomElement(DomainModel.DicomTag.SERIESINSTANCEUID, seriesInstanceUid.ToString()),
                        new DomainModel.DicomElement(DomainModel.DicomTag.SOPINSTANCEUID, sopInstanceUid.ToString()),
                        new DomainModel.DicomElement(DomainModel.DicomTag.MODALITY, "CT"),
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
            var service = new DicomApplicationService(mockRepository.Object, dicomParser);

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
            var testController = 
                new DicomController(service, new NullLogger<DicomController>()) 
                { 
                    ControllerContext = new ControllerContext()
                    {
                        HttpContext = httpContext,
                    }
                };

            var okObjectResult = (OkObjectResult)testController.GetDicomInstance(seriesInstanceUid.ToString(), sopInstanceUid.ToString()).Result;
            var retrievedDicomInstance = (AbstractionModel.DicomInstance)okObjectResult.Value;

            Assert.AreEqual(sopInstanceUid.ToString(), retrievedDicomInstance.SopInstanceUid);

            var retrievedPatientId = 
                retrievedDicomInstance.DicomElements.Single(element => 
                    element.DicomTag.Equals(DomainModel.DicomTag.PATIENTID.ToString()));
            Assert.AreEqual(patientId, retrievedPatientId.Value);

            var retrievedPatientName =
                retrievedDicomInstance.DicomElements.Single(element => 
                    element.DicomTag.Equals(DomainModel.DicomTag.PATIENTNAME.ToString()));
            Assert.AreEqual(patientId, retrievedPatientId.Value);

        }
    }
}
