using System;
using System.Collections.Generic;
using System.Linq;

namespace TestDicomDomainMapper
{

    class Program
    {
        static void CreatePopulateEFModel()
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

        /// <summary>
        /// creates a new series aggregate
        /// </summary>
        /// <returns></returns>
        static DomainModel.DicomSeries CreateSeries()
        {
            var series =
                DomainModel.DicomSeries.Create(
                    DomainModel.DicomUid.Create("1.2.3.7"),
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
        static void AddInstancesToSeries(DomainModel.DicomSeries series)
        {
            for (int n = 0; n < 3; n++)
            {
                var attributes = new List<DomainModel.DicomAttribute>()
                {
                    DomainModel.DicomAttribute.Create(DomainModel.DicomTag.PATIENTID, "98765"),
                    DomainModel.DicomAttribute.Create(DomainModel.DicomTag.ACQUISITIONDATETIME, (new DateTime(2021,01,02)).ToString()),
                    DomainModel.DicomAttribute.Create(DomainModel.DicomTag.MODALITY, "CT"),
                };

                var instance =
                    DomainModel.DicomInstance.Create(
                        DomainModel.DicomUid.Create($"1.2.3.{n + 7}"),
                        attributes);

                series.AddInstance(instance);
            }
        }

        static void MapAggregateToEFModel(EFModel.MyContext context, DomainModel.DicomSeries fromSeries)
        {
            // see if series is already present
            var matchSeries = 
                context.DicomSeries.Where(series => 
                    series.SeriesInstanceUid.CompareTo(fromSeries.SeriesInstanceUid.UidString) == 0)
                    .SingleOrDefault();
            if (matchSeries == null)
            {
                matchSeries = 
                    new EFModel.DicomSeries() 
                    { 
                        SeriesInstanceUid = fromSeries.SeriesInstanceUid.UidString,
                        AcquisitionDateTime = fromSeries.AcquisitionDateTime,
                        Modality = fromSeries.Modality,
                        PatientID = fromSeries.PatientId,
                    };
                context.DicomSeries.Add(matchSeries);
            }
            else
            {
                // otherwise we are updating
            }

            // see if instances need mapping
            foreach (var fromInstance in fromSeries.Instances)
            {
                var matchInstance =
                        context.DicomInstances.Where(instance =>
                            instance.SopInstanceUid.CompareTo(fromInstance.SopInstanceUid.UidString) == 0)
                            .SingleOrDefault();
                if (matchInstance == null)
                {
                    var newInstance =
                        new EFModel.DicomInstance()
                        {
                            SopInstanceUid = fromInstance.SopInstanceUid.UidString,
                            DicomSeries = matchSeries,
                        };

                    var convertedAttributes =
                        fromInstance.DicomAttributes.Select(fromAttribute =>
                            new EFModel.DicomAttribute()
                            {
                                Tag = fromAttribute.DicomTag.ToString(),
                                Value = fromAttribute.Value,
                                DicomInstance = newInstance,
                            }).ToList();
                    newInstance.Attributes = convertedAttributes;

                    context.DicomInstances.Add(newInstance);
                    context.DicomAttributes.AddRange(convertedAttributes);
                }
                else
                {
                    // can't modify an existing DicomInstance
                    throw new NotSupportedException();
                }
            }
        }

        static DomainModel.DicomSeries MapEFModelToAggregate(EFModel.DicomSeries fromSeries)
        {
            var instances =
                fromSeries.Instances?.Select(fromInstance =>
                    DomainModel.DicomInstance.Create(
                        DomainModel.DicomUid.Create(fromInstance.SopInstanceUid),
                        fromInstance.Attributes?.Select(fromAttribute =>
                            DomainModel.DicomAttribute.Create(
                                DomainModel.DicomTag.Create(fromAttribute.Tag),
                                fromAttribute.Value))));

            var newSeries =
                DomainModel.DicomSeries.Create(
                    DomainModel.DicomUid.Create(fromSeries.SeriesInstanceUid),
                    fromSeries.PatientID,
                    fromSeries.Modality,
                    fromSeries.AcquisitionDateTime,
                    instances);

            return newSeries;
        }

        static void Main(string[] args)
        {
            // 1. try just creating an EF model
            // CreatePopulateEFModel();

            // 2.
            DomainModel.DicomUid newSeriesUid = null;
            {
                var context = new EFModel.MyContext();

                var newSeriesDomainModel = CreateSeries();

                MapAggregateToEFModel(context, newSeriesDomainModel);

                context.SaveChanges();

                // capture new series ID
                newSeriesUid = newSeriesDomainModel.SeriesInstanceUid;
            }

            // 3.
            {
                var context = new EFModel.MyContext();

                var newSeriesEFModel =
                    context.DicomSeries.First(series =>
                        series.SeriesInstanceUid.CompareTo(newSeriesUid.UidString) == 0);
                var newSeriesDomainModel = MapEFModelToAggregate(newSeriesEFModel);

                AddInstancesToSeries(newSeriesDomainModel);

                MapAggregateToEFModel(context, newSeriesDomainModel);

                context.SaveChanges();
            }
        }
    }
}
