using System.Collections.Generic;

namespace Elektrum.Capability.Dicom.Infrastructure.EFModel
{
    public class DicomInstance
    {
        public int ID { get; set; }

        public int DicomSeriesId { get; set; }

        public string SopInstanceUid { get; set; }

        public List<DicomAttribute> DicomAttributes { get; set; } =
            new List<DicomAttribute>();
    }
}
