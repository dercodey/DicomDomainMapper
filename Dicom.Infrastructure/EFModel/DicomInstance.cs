using AutoMapper;
using System.Collections.Generic;

namespace Dicom.Infrastructure.EFModel
{
    public class DicomInstance
    {
        [IgnoreMap]
        public int ID { get; set; }

        [IgnoreMap]
        public int DicomSeriesId { get; set; }

        public string SopInstanceUid { get; set; }

        [IgnoreMap]
        public DicomSeries DicomSeries { get; set; }

        public List<DicomAttribute> DicomAttributes { get; set; } =
            new List<DicomAttribute>();
    }
}
