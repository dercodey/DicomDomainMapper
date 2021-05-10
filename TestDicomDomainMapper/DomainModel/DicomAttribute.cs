using System;
using System.Diagnostics.CodeAnalysis;

namespace TestDicomDomainMapper.DomainModel
{
    /// <summary>
    /// value object representing a DICOM attribute
    /// </summary>
    class DicomAttribute : IValueObject, IEquatable<DicomAttribute>
    {
        public DicomTag DicomTag { get; private set; }

        public string Value { get; private set; }

        public static DicomAttribute Create(DicomTag tag, string value)
        {
            // validate value???
            return new DicomAttribute() 
            { 
                DicomTag = tag, 
                Value = value 
            };
        }

        public bool Equals([AllowNull] DicomAttribute other)
        {
            return DicomTag == other.DicomTag && Value == other.Value;
        }
    }
}
