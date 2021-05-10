using AutoMapper;

namespace TestDicomDomainMapper.EFModel
{
    public class DicomAttribute
    {
        [IgnoreMap]
        public int ID { get; set; }
        [IgnoreMap]
        public int DicomInstanceId{ get; set; }
        public string DicomTag { get; set; }
        public string Value { get; set; }
        [IgnoreMap]
        public DicomInstance DicomInstance { get; set; }
    }
}
