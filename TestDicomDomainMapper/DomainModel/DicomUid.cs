using System;
using System.Diagnostics.CodeAnalysis;

namespace TestDicomDomainMapper.DomainModel
{
    /// <summary>
    /// value object representing Dicom UID
    /// </summary>
    public class DicomUid : Seedworks.IValueObject, IEquatable<DicomUid>
    {
        private string _uidString;

        public DicomUid(string uidString)
        {
            // TODO: test to see if string is in proper format

            // TODO: validate string is correct

            _uidString = uidString;
        }

        #region IEquatable

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals([AllowNull] DicomUid other)
        {
            return _uidString.CompareTo(other._uidString) == 0;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _uidString;
        }
    }
}
