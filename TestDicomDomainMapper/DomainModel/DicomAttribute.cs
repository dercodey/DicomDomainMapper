using AutoMapper;
using System;
using System.Diagnostics.CodeAnalysis;

namespace TestDicomDomainMapper.DomainModel
{
    /// <summary>
    /// value object representing a DICOM attribute
    /// </summary>
    class DicomAttribute : IValueObject, IEquatable<DicomAttribute>
    {
        [IgnoreMap]
        public DicomTag DicomTag { get; private set; }

        public string Value { get; private set; }

        public DicomAttribute(DicomTag dicomTag, string value)
        {
            DicomTag = dicomTag;
            Value = value;
        }

        public bool Equals([AllowNull] DicomAttribute other)
        {
            return DicomTag == other.DicomTag && Value == other.Value;
        }
    }
}
