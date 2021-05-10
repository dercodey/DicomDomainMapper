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

        public static DicomUid Create(string uidString)
        {
            // test to see if string is in proper format

            // validate string is correct
            var newDicomUid = new DicomUid();
            newDicomUid._uidString = uidString;
            return newDicomUid;
        }

        public bool Equals([AllowNull] DicomUid other)
        {
            return _uidString.CompareTo(other._uidString) == 0;
        }
    }
}
