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
        /// 
        /// </summary>
        /// <param name="dicomTag"></param>
        /// <param name="value"></param>
        public DicomAttribute(DicomTag dicomTag, string value)
        {
            DicomTag = dicomTag;
            Value = value;
        }

        /// <summary>
        /// 
        /// </summary>
        [IgnoreMap]
        public DicomTag DicomTag { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string Value { get; private set; }

        #region IEquatable

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals([AllowNull] DicomAttribute other)
        {
            return DicomTag == other.DicomTag && Value == other.Value;
        }

        #endregion
    }
}
