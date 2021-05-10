using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestDicomDomainMapper
{
    static class MappingExtensions
    {
        /// <summary>
        /// maps from the domail model to the EF model
        /// </summary>
        /// <param name="context">db context to be used for mapping</param>
        /// <param name="fromSeries">series to map</param>
        public static EFModel.DicomSeries ToEFModel(this DomainModel.DicomSeries fromSeries, EFModel.MyContext context)
        {
            // see if series is already present
            var matchSeries =
                context.DicomSeries.Where(series =>
                    series.SeriesInstanceUid.CompareTo(fromSeries.SeriesInstanceUid.ToString()) == 0)
                    .SingleOrDefault();
            if (matchSeries == null)
            {
                matchSeries =
                    new EFModel.DicomSeries()
                    {
                        SeriesInstanceUid = fromSeries.SeriesInstanceUid.ToString(),
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
            foreach (var fromInstance in fromSeries.DicomInstances)
            {
                var matchInstance =
                        context.DicomInstances.Where(instance =>
                            instance.SopInstanceUid.CompareTo(fromInstance.SopInstanceUid.ToString()) == 0)
                            .SingleOrDefault();
                if (matchInstance != null)
                {
                    // can't modify an existing DicomInstance
                    throw new NotSupportedException();
                }

                var newInstance =
                    new EFModel.DicomInstance()
                    {
                        SopInstanceUid = fromInstance.SopInstanceUid.ToString(),
                        DicomSeries = matchSeries,
                    };
                context.DicomInstances.Add(newInstance);

                var convertedAttributes =
                    fromInstance.DicomAttributes.Select(fromAttribute =>
                        new EFModel.DicomAttribute()
                        {
                            DicomTag = fromAttribute.DicomTag.ToString(),
                            Value = fromAttribute.Value,
                            DicomInstance = newInstance,
                        }).ToList();
                newInstance.DicomAttributes = convertedAttributes;
                context.DicomAttributes.AddRange(convertedAttributes);
            }

            return matchSeries;
        }

        public static DomainModel.DicomSeries ToDomainModel(this EFModel.DicomSeries fromSeries)
        {
            var instances =
                fromSeries.DicomInstances?.Select(fromInstance =>
                    DomainModel.DicomInstance.Create(
                        fromInstance.SopInstanceUid,
                        fromInstance.DicomAttributes?.Select(fromAttribute =>
                            DomainModel.DicomAttribute.Create(
                                fromAttribute.DicomTag,
                                fromAttribute.Value))));

            var newSeries =
                DomainModel.DicomSeries.Create(
                    fromSeries.SeriesInstanceUid,
                    fromSeries.PatientID,
                    fromSeries.Modality,
                    fromSeries.AcquisitionDateTime,
                    instances);

            return newSeries;
        }
    }
}
