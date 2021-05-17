using Dicom.Api.Controllers;
using Dicom.Application.Repositories;
using Dicom.Application.Services;
using DomainModel = Dicom.Domain.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace Dicom.Api.Test
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

            var _mockService = new Mock<IDicomApplicationService>();
            _mockService.Setup(svc => 
                svc.GetAllSeriesForPatient(It.Is<string>(s => 
                    s.Equals(testPatientId))))
                .Returns(testDicomSerieses);

            var _testController = new DicomController(_mockService.Object, new NullLogger<DicomController>());
            var okObjectResult = (OkObjectResult)_testController.GetAllDicomSeriesForPatient(testPatientId).Result;
            var listDicomSeries = (IEnumerable<Abstractions.DicomSeries>)okObjectResult.Value;

            Assert.AreEqual(5, listDicomSeries.Count());
            foreach (var dicomSeries in listDicomSeries)
            {
                Assert.AreEqual(testPatientId, dicomSeries.PatientId);
                Assert.AreEqual(testPatientName, dicomSeries.PatientName);
                Assert.AreEqual(DomainModel.Modality.CT.ToString(), dicomSeries.Modality);
                Assert.AreEqual(testAcquistionDateTime, dicomSeries.AcquisitionDateTime);
            }

            var allDistinctSeriesInstanceUids = 
                listDicomSeries.Select(dicomSeries => dicomSeries.SeriesInstanceUid).Distinct();
            Assert.AreEqual(5, allDistinctSeriesInstanceUids.Count());
        }

        [TestMethod]
        public void TestGetDicomSeries()
        {
            var testPatientName = "Last, First";
            var testPatientId = "98765";
            var testSeriesInstanceUid = new DomainModel.DicomUid("1.2.3.7");
            var testAcquistionDateTime = DateTime.Now;
            var testDicomSeries =
                new DomainModel.DicomSeries(testSeriesInstanceUid, 
                    testPatientName, testPatientId, DomainModel.Modality.CT, testAcquistionDateTime, 3, null);

            var _mockService = new Mock<IDicomApplicationService>();
            _mockService.Setup(svc => 
                    svc.GetSeriesByUid(It.Is<DomainModel.DicomUid>(s => 
                        s.Equals(testSeriesInstanceUid))))
                .Returns(testDicomSeries);

            var _testController = new DicomController(_mockService.Object, new NullLogger<DicomController>());

            var okObjectResult = (OkObjectResult)_testController.GetDicomSeries(testPatientId, testSeriesInstanceUid.ToString()).Result;
            var abDicomSeries = (Abstractions.DicomSeries)okObjectResult.Value;

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
            var testAbDicomSeries = 
                new Abstractions.DicomSeries() 
                { 
                    Modality = "CT",
                };

            var _mockService = new Mock<IDicomApplicationService>();
            _mockService.Setup(svc => 
                    svc.CreateSeriesAsync(It.IsAny<DomainModel.DicomSeries>()))
                .Returns(Task.CompletedTask)
                .Callback((DomainModel.DicomSeries s) => 
                {
                    s.Modality.ToString().Equals(testAbDicomSeries.Modality);
                });

            var _testController = new DicomController(_mockService.Object, new NullLogger<DicomController>());
            _testController.AddDicomSeries("", testAbDicomSeries).Wait();
        }

        [TestMethod]
        public void TestDeleteDicomSeries()
        {
        }

        [TestMethod]
        public void TestAddDicomInstance()
        {
        }

        [TestMethod]
        public void TestGetDicomInstance()
        {
        }
    }
}
