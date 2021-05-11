using System;
using System.Diagnostics.CodeAnalysis;

namespace TestDicomDomainMapper.DomainModel
{
    /// <summary>
    /// value object representing Dicom UID
    /// </summary>
    class DicomUid : IValueObject, IEquatable<DicomUid>
    {
        private string _uidString;

        public override string ToString()
        {
            return _uidString;
        }

        public DicomUid(string uidString)
        {
            // test to see if string is in proper format

            // validate string is correct

            _uidString = uidString;
        }

        public bool Equals([AllowNull] DicomUid other)
        {
            return _uidString.CompareTo(other._uidString) == 0;
        }
    }
}
