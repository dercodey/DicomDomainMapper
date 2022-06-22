using System;
using System.Diagnostics.CodeAnalysis;
using Elekta.Capability.Dicom.Domain.Seedwork;

namespace Elekta.Capability.Dicom.Domain.Model
{
    /// <summary>
    /// A DicomAttribute is a value object in the DICOM domain model, which represents a single DICOM element.  It is an immutable representation of a particular attribute by tag and value.
    /// </summary>
    public class DicomAttribute : IValueObject, IEquatable<DicomAttribute>
    {
        /// <summary>
        /// The DicomAttribute constructor builds an attribute for a given tag and value.  The tag and value are immutable properties of the DicomAttribute (which is consistent with the definition of a value object)
        /// </summary>
        /// <param name="dicomTag">The DicomTag to use for the new attribute</param>
        /// <param name="value">The DicomValue to use for the new attribute</param>
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
        /// Each DicomAtribute has a DICOM tag that indicates the identity of the attribute.
        /// </summary>
        public DicomTag DicomTag 
        { 
            get; 
            private set; 
        }

        /// <summary>
        /// The value for the attribute is encoded as a string, which is an easy approach for a prototype.
        /// </summary>
        public string Value 
        { 
            get; 
            private set; 
        }

        #region IEquatable

        /// <summary>
        /// A DicomAttribute can be compared to another DicomAttribute for structural equality, which is met if the tag and value both match.
        /// </summary>
        /// <param name="other">the other DicomAttribute to be compared to this one</param>
        /// <returns>true if this DicomAttribute is equal to the other DicomAttribute</returns>
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
