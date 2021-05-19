namespace Elekta.Capability.Dicom.Infrastructure.EFModel
{
    public class DicomAttribute
    {
        public int ID { get; set; }

        public int DicomInstanceId{ get; set; }

        public string DicomTag { get; set; }

        public string Value { get; set; }
    }
}
