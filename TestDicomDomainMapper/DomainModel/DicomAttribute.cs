using System;
using System.Collections.Generic;
using System.Text;

namespace TestDicomDomainMapper.DomainModel
{
    class DicomAttribute : IValueObject
    {
        public DicomTag DicomTag { get; private set; }

        public string Value { get; private set; }

        public static DicomAttribute Create(DicomTag tag, string value)
        {
            // validate value???
            return new DicomAttribute() { DicomTag = tag, Value = value };
        }
    }
}
