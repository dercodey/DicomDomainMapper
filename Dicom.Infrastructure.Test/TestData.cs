using System;
using System.Collections.Generic;
using Elekta.Capability.Dicom.Domain.Model;

namespace Elekta.Capability.Dicom.Infrastructure.Test
{
    internal static class TestData
    {
        /// <summary>
        /// creates a new series aggregate
        /// </summary>
        /// <returns></returns>
        internal static DicomSeries CreateSeries()
        {
            var series =
                new DicomSeries(
                    new DicomUid("1.2.3.7"),
                    "Last, First",
                    "98765",
                    Modality.CT,
                    new DateTime(2021, 01, 02),
                    3, // expected instance count
                    new List<DicomInstance>());

            return series;
        }

        /// <summary>
        /// creates a few new instances and adds to the series
        /// </summary>
        /// <param name="series"></param>
        internal static void AddInstancesToSeries(DicomSeries series)
        {
            for (int n = 0; n < 3; n++)
            {
                var sopInstanceUid = new DicomUid($"1.2.3.{n + 7}");
                var attributes = new List<DicomAttribute>()
                {
                    new DicomAttribute(DicomTag.PATIENTNAME, "Last, First"),
                    new DicomAttribute(DicomTag.PATIENTID, "98765"),
                    new DicomAttribute(DicomTag.SERIESINSTANCEUID, series.SeriesInstanceUid.ToString()),
                    new DicomAttribute(DicomTag.MODALITY, Modality.CT.ToString()),
                    new DicomAttribute(DicomTag.ACQUISITIONDATETIME, (new DateTime(2021,01,02)).ToString()),
                    new DicomAttribute(DicomTag.SOPINSTANCEUID, sopInstanceUid.ToString()),
                };

                var instance =
                    new DicomInstance(
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
            var context = new EFModel.DicomDbContext();
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
