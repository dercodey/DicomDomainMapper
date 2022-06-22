using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Elektrum.Capability.Dicom.Domain.Seedwork;

namespace Elektrum.Capability.Dicom.Domain.Model
{
    /// <summary>
    /// value object representing Dicom UID
    /// </summary>
    public class DicomUid : IValueObject, IEquatable<DicomUid>
    {
        private string _uidString;

        /// <summary>
        /// constructs from a string representing the UID.
        /// this example requires the string to have four numeric fields
        /// </summary>
        /// <param name="uidString"></param>
        public DicomUid(string uidString)
        {
#if TEST_UID_FORMAT
            // validate string is correct
            var pattern = "[0-9.]*";
            var matches = Regex.Matches(uidString, pattern);
            if (matches.Count != 1)
            {
                throw new ArgumentException("Incorrect format for DICOM UID");
            }
#endif

            _uidString = uidString;
        }

#region IEquatable

        /// <summary>
        /// perform equality test on two UIDs by comparing their string representation
        /// </summary>
        /// <param name="other">the other UID to compare</param>
        /// <returns>true if the are equal</returns>
        public bool Equals([AllowNull] DicomUid other)
        {
            return _uidString.CompareTo(other._uidString) == 0;
        }

#endregion

        /// <summary>
        /// string representation of the UID
        /// </summary>
        /// <returns>UID as a string</returns>
        public override string ToString()
        {
            return _uidString;
        }
    }
}
