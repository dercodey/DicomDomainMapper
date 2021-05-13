using System;
using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using Dicom.Domain.Seedwork;

namespace Dicom.Domain.Model
{
    /// <summary>
    /// value object representing a DICOM attribute
    /// </summary>
    public class DicomAttribute : IValueObject, IEquatable<DicomAttribute>
    {
        /// <summary>
        /// construct an attribute from a tag and value
        /// </summary>
        /// <param name="dicomTag"></param>
        /// <param name="value"></param>
        public DicomAttribute(DicomTag dicomTag, string value)
        {
            if (dicomTag == null)
            {
                throw new ArgumentNullException("DICOM Tag must be provided");
            }

            if (!dicomTag.CheckValue(value))
            {
                throw new ArgumentException($"Invalid value for DICOM Tag {dicomTag}");
            }

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
