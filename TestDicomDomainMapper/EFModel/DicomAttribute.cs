namespace TestDicomDomainMapper.EFModel
{
    public class DicomAttribute
    {
        public int ID { get; set; }
        public int DicomInstanceId{ get; set; }
        public string Tag { get; set; }
        public string Value { get; set; }
        public DicomInstance DicomInstance { get; set; }
    }
}
