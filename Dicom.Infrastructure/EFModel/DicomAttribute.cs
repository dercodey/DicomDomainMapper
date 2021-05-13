namespace Dicom.Infrastructure.EFModel
{
    public class DicomAttribute
    {
        public int ID { get; set; }

        [AutoMapper.IgnoreMap]
        public int DicomInstanceId{ get; set; }

        public string DicomTag { get; set; }

        public string Value { get; set; }

        [AutoMapper.IgnoreMap]
        public DicomInstance DicomInstance { get; set; }
    }
}
