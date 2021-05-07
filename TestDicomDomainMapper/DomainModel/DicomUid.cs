namespace TestDicomDomainMapper.DomainModel
{
    /// <summary>
    /// value object representing Dicom UID
    /// </summary>
    class DicomUid : IValueObject
    {
        private string _uidString;

        public override string ToString()
        {
            return _uidString;
        }

        public static DicomUid Create(string uidString)
        {
            // test to see if string is in proper format

            // validate string is correct
            var newDicomUid = new DicomUid();
            newDicomUid._uidString = uidString;
            return newDicomUid;
        }
    }
}
