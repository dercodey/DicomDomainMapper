namespace TestDicomDomainMapper.DomainModel
{
    /// <summary>
    /// value object representing a DICOM attribute
    /// </summary>
    class DicomAttribute : IValueObject
    {
        public DicomTag DicomTag { get; private set; }

        public string Value { get; private set; }

        public static DicomAttribute Create(DicomTag tag, string value)
        {
            // validate value???
            return new DicomAttribute() 
            { 
                DicomTag = tag, 
                Value = value 
            };
        }
    }
}
