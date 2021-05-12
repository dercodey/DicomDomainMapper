using System.Collections.Generic;

namespace Dicom.Infrastructure.EFModel
{
    public class DicomInstance
    {
        [AutoMapper.IgnoreMap]
        public int ID { get; set; }

        [AutoMapper.IgnoreMap]
        public int DicomSeriesId { get; set; }

        public string SopInstanceUid { get; set; }

        [AutoMapper.IgnoreMap]
        public DicomSeries DicomSeries { get; set; }

        public List<DicomAttribute> DicomAttributes { get; set; } =
            new List<DicomAttribute>();
    }
}
