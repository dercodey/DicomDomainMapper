using System;
using System.Collections.Generic;
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
        private readonly Type _valueRepresentation;

        /// <summary>
        /// construct a tag from the group/element representation
        /// </summary>
        /// <param name="group">group number for the tag</param>
        /// <param name="element">element number for the tag</param>
        /// <param name="valueRepresentation">type that the value must conform to</param>
        /// <param name="allowPrivate">indicates whether private is allowed</param>
        public DicomTag(ushort group, ushort element, Type valueRepresentation, bool allowPrivate)
        {
            if (!allowPrivate
                && group % 2 == 1)
            {
                throw new ArgumentException("DICOM Tag group must be even for non-private tags");
            }

            if (valueRepresentation == null)
            {
                throw new ArgumentException("Value Representation must be provided");
            }

            Group = group;
            Element = element;

            _valueRepresentation = valueRepresentation;
        }

        /// <summary>
        /// converts a string to tag value
        /// </summary>
        /// <param name="strTag">tag in '(xxxx,xxxx)' format</param>
        /// <param name="valueRepresentation">type that the value must conform to</param>
        /// <param name="allowPrivate">indicates whether private is allowed</param>
        public DicomTag(string strTag, Type valueRepresentation, bool allowPrivate)
        {
            var pattern = @"\((?<group>[0-9A-F]{4}),(?<element>[0-9A-F]{4})\)";
            var matches = Regex.Match(strTag, pattern);
            
            if (!matches.Groups.ContainsKey("group")
                || !matches.Groups.ContainsKey("element"))
            {
                throw new ArgumentException("Incorrect format for DICOM tag");
            }

            if (valueRepresentation == null)
            {
                throw new ArgumentException("Value Representation must be provided");
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

            _valueRepresentation = valueRepresentation;
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

        /// <summary>
        /// helper to check if a value is valid for this tag
        /// </summary>
        /// <param name="value">value to be checked</param>
        /// <returns></returns>
        public bool CheckValue(string value)
        {
            if (_valueRepresentation == typeof(string))
            {
                return true;
            }
            else if (_valueRepresentation == typeof(DateTime))
            {
                DateTime result;
                var success = DateTime.TryParse(value, out result);
                return success;
            }
            else if (_valueRepresentation == typeof(DicomUid))
            {
                try
                {
                    new DicomUid(value);
                }
                catch (ArgumentException)
                {
                    return false;
                }

                return true;
            }

            return false;
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
        public static DicomTag MODALITY = new DicomTag(0x0008, 0x0060, typeof(string), false);
        public static DicomTag PATIENTNAME = new DicomTag(0x0010, 0x0010, typeof(string), false);
        public static DicomTag PATIENTID = new DicomTag(0x0010, 0x0020, typeof(string), false);
        public static DicomTag SOPINSTANCEUID = new DicomTag("(0008,0018)", typeof(DicomUid), false);
        public static DicomTag SERIESINSTANCEUID = new DicomTag("(0020,000E)", typeof(DicomUid), false);
        public static DicomTag ACQUISITIONDATETIME = new DicomTag("(0008,002A)", typeof(DateTime), false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matchTag"></param>
        /// <returns></returns>
        public static DicomTag GetTag(string matchTag)
        {
            var listOfTags = new List<DicomTag>
            {
                MODALITY,
                PATIENTNAME,
                PATIENTID,
                SOPINSTANCEUID,
                SERIESINSTANCEUID,
                ACQUISITIONDATETIME
            };

            foreach (var tag in listOfTags)
            {
                if (tag.ToString().CompareTo(matchTag) == 0)
                {
                    return tag;
                }
            }

            return null;
        }
    }
}