using System;
using System.Diagnostics.CodeAnalysis;

namespace TestDicomDomainMapper.DomainModel
{
    public class DicomTag : IValueObject, IEquatable<DicomTag>
    {
        public ushort Group { get; private set; }
        public ushort Element { get; private set; }

        public static DicomTag Create(ushort group, ushort element)
        {
            // validate group and element values are valid

            return new DicomTag() { Group = group, Element = element };
        }

        public static DicomTag Create(string strTag)
        {
            return new DicomTag();
        }

        public static DicomTag PATIENTID = Create(0x0010, 0x0010);
        public static DicomTag MODALITY = Create(0x0010, 0x0010);
        public static DicomTag SOPINSTANCEUID= Create(0x0010, 0x0010);
        public static DicomTag SERIESINSTANCEUID = Create(0x0010, 0x0010);
        public static DicomTag ACQUISITIONDATETIME = Create(0x0010, 0x0010);

        public bool Equals([AllowNull] DicomTag other)
        {
            return Group == other.Group && Element == other.Element;
        }

        public override string ToString()
        {
            return $"{Group:X2}{Element:X2}";
        }
    }
}