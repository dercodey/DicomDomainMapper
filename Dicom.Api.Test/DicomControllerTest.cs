using Dicom.Api.Controllers;
using Dicom.Application.Repositories;
using Dicom.Application.Services;
using DomainModel = Dicom.Domain.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;

namespace Dicom.Api.Test
{
    [TestClass]
    public class DicomControllerTest
    {
        [TestMethod]
        public void TestGetDicomSeries()
        {
            var testSeriesInstanceUid = new DomainModel.DicomUid("1.2.3.7");
            var testDicomSeries = 
                new DomainModel.DicomSeries(testSeriesInstanceUid, 
                    "Last, First", "98765", DomainModel.Modality.CT, DateTime.Now, 3, null);

            var _mockService = new Mock<IDicomApplicationService>();
            _mockService.Setup(svc => svc.GetSeriesByUid(testSeriesInstanceUid)).Returns(testDicomSeries);

            var _testController = new DicomController(_mockService.Object, new NullLogger<DicomController>());

            var result = _testController.GetDicomSeries(testSeriesInstanceUid.ToString());
        }


        [TestMethod]
        public void TestAddDicomSeries()
        {
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
