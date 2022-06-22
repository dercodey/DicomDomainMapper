using System;
using System.Collections.Generic;
using DomainModel = Elektrum.Capability.Dicom.Domain.Model;
using EFModel = Elektrum.Capability.Dicom.Infrastructure.EFModel;

namespace TestDicomDomainMapper
{
    internal static class TestData
    {
        /// <summary>
        /// creates a new series aggregate
        /// </summary>
        /// <returns></returns>
        internal static DomainModel.DicomSeries CreateSeries()
        {
            var series =
                new DomainModel.DicomSeries(
                    new DomainModel.DicomUid("1.2.3.7"),
                    "98765",
                    "CT",
                    new DateTime(2021, 01, 02),
                    new List<DomainModel.DicomInstance>());

            return series;
        }

        /// <summary>
        /// creates a few new instances and adds to the series
        /// </summary>
        /// <param name="series"></param>
        internal static void AddInstancesToSeries(DomainModel.DicomSeries series)
        {
            for (int n = 0; n < 3; n++)
            {
                var sopInstanceUid = new DomainModel.DicomUid($"1.2.3.{n + 7}");
                var attributes = new List<DomainModel.DicomAttribute>()
                {
                    new DomainModel.DicomAttribute(DomainModel.DicomTag.PATIENTID, "98765"),
                    new DomainModel.DicomAttribute(DomainModel.DicomTag.SERIESINSTANCEUID, series.SeriesInstanceUid.ToString()),
                    new DomainModel.DicomAttribute(DomainModel.DicomTag.MODALITY, "CT"),
                    new DomainModel.DicomAttribute(DomainModel.DicomTag.ACQUISITIONDATETIME, (new DateTime(2021,01,02)).ToString()),
                    new DomainModel.DicomAttribute(DomainModel.DicomTag.SOPINSTANCEUID, sopInstanceUid.ToString()),
                };

                var instance =
                    new DomainModel.DicomInstance(
                        sopInstanceUid,
                        attributes);

                series.AddInstance(instance);
            }
        }

        /// <summary>
        /// creates an EF model and uses to populate DB
        /// </summary>
        internal static void CreatePopulateEFModel()
        {
            var context = new EFModel.MyContext();
            var series = new EFModel.DicomSeries()
            {
                SeriesInstanceUid = "1.2.5.4",
                PatientID = "12345",
                Modality = "CT",
                AcquisitionDateTime = DateTime.Now
            };
            context.DicomSeries.Add(series);

            context.DicomInstances.Add(
                new EFModel.DicomInstance()
                {
                    DicomSeries = series,
                    SopInstanceUid = "1.2.5.5",
                });

            context.DicomInstances.Add(
                new EFModel.DicomInstance()
                {
                    DicomSeries = series,
                    SopInstanceUid = "1.2.5.6",
                });

            context.DicomInstances.Add(
                new EFModel.DicomInstance()
                {
                    DicomSeries = series,
                    SopInstanceUid = "1.2.5.7",
                });

            context.SaveChanges();
        }
    }
}
