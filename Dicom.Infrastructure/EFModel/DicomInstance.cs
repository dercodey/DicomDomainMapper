using System.Collections.Generic;

namespace Dicom.Infrastructure.EFModel
{
    public class DicomInstance
    {
        public int ID { get; set; }

        public int DicomSeriesId { get; set; }

        public string SopInstanceUid { get; set; }

        public DicomSeries DicomSeries { get; set; }

        public List<DicomElement> DicomElements { get; set; } =
            new List<DicomElement>();
    }
}
