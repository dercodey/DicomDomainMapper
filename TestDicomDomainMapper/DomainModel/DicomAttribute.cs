using AutoMapper;
using System;
using System.Diagnostics.CodeAnalysis;

namespace TestDicomDomainMapper.DomainModel
{
    /// <summary>
    /// value object representing a DICOM attribute
    /// </summary>
    public class DicomAttribute : Seedworks.IValueObject, IEquatable<DicomAttribute>
    {
        /// <summary>
        /// construct an attribute from a tag and value
        /// </summary>
        /// <param name="dicomTag"></param>
        /// <param name="value"></param>
        public DicomAttribute(DicomTag dicomTag, string value)
        {
            // TODO: check that values is consistent with tag VR

            DicomTag = dicomTag;
            Value = value;
        }

        /// <summary>
        /// tag for the attribute
        /// </summary>
        [IgnoreMap]
        public DicomTag DicomTag 
        { 
            get; 
            private set; 
        }

        /// <summary>
        /// value for the attribute
        /// </summary>
        public string Value 
        { 
            get; 
            private set; 
        }

        #region IEquatable

        /// <summary>
        /// compares two attributes for equality
        /// </summary>
        /// <param name="other">the other attribute to be compared</param>
        /// <returns>true if equal</returns>
        public bool Equals([AllowNull] DicomAttribute other)
        {
            if (other == null)
            {
                return false;
            }

            return DicomTag == other.DicomTag && Value == other.Value;
        }

        #endregion
    }
}
