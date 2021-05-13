using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Dicom.Domain.Seedwork;

namespace Dicom.Domain.Model
{
    /// <summary>
    /// value type for a Dicom Tag
    /// </summary>
    public class DicomTag : IValueObject, IEquatable<DicomTag>
    {
        /// <summary>
        /// construct a tag from the group/element representation
        /// </summary>
        /// <param name="group">group number for the tag</param>
        /// <param name="element">element number for the tag</param>
        /// <param name="allowPrivate">indicates whether private is allowed</param>
        public DicomTag(ushort group, ushort element, bool allowPrivate = false)
        {
            if (!allowPrivate
                && group % 2 == 1)
            {
                throw new ArgumentException("DICOM Tag group must be even for non-private tags");
            }

            Group = group;
            Element = element;
        }

        /// <summary>
        /// converts a string to tag value
        /// </summary>
        /// <param name="strTag">tag in '(xxxx,xxxx)' format</param>
        /// <param name="allowPrivate">indicates whether private is allowed</param>
        public DicomTag(string strTag, bool allowPrivate = false)
        {
            var pattern = @"\((?<group>[0-9A-F]{4}),(?<element>[0-9A-F]{4})\)";
            var matches = Regex.Match(strTag, pattern);
            
            if (!matches.Groups.ContainsKey("group")
                || !matches.Groups.ContainsKey("element"))
            {
                throw new ArgumentException("Incorrect format for DICOM tag");
            }

            var group = ushort.Parse(matches.Groups["group"].Value,
                    System.Globalization.NumberStyles.HexNumber);
            if (!allowPrivate
                && group % 2 == 1)
            {
                throw new ArgumentException("DICOM Tag group must be even for non-private tags");
            }

            // get the group and element from matches
            Group = group;
            Element = ushort.Parse(matches.Groups["element"].Value, 
                    System.Globalization.NumberStyles.HexNumber);
        }

        /// <summary>
        /// the group number for the tag
        /// </summary>
        public ushort Group 
        { 
            get; 
            private set; 
        }

        /// <summary>
        /// the element number for the tag
        /// </summary>
        public ushort Element 
        { 
            get; 
            private set; 
        }

        #region IEquatable

        /// <summary>
        /// compares to another tag to see if they are equal
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals([AllowNull] DicomTag other)
        {
            if (other == null)
            {
                return false;
            }

            return Group == other.Group && Element == other.Element;
        }
        #endregion

        /// <summary>
        /// output the tag as '(XXXX,XXXX)' format string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"({Group:X4},{Element:X4})";
        }

        // a few common tags
        public static DicomTag MODALITY = new DicomTag(0x0008, 0x0060);
        public static DicomTag PATIENTID = new DicomTag(0x0010, 0x0020);
        public static DicomTag SOPINSTANCEUID = new DicomTag("(0008,0018)");
        public static DicomTag SERIESINSTANCEUID = new DicomTag("(0020,000E)");
        public static DicomTag ACQUISITIONDATETIME = new DicomTag("(0008,002A)");
    }
}