using System.Collections.Generic;

namespace TestDicomDomainMapper.EFModel
{
    public class DicomInstance
    {
        public int ID { get; set; }
        public int DicomSeriesId { get; set; }
        public string SopInstanceUid { get; set; }
        public DicomSeries DicomSeries { get; set; }
        public List<DicomAttribute> Attributes { get; set; }
    }
}
