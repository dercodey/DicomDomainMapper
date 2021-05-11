using System;
using System.Diagnostics.CodeAnalysis;

namespace TestDicomDomainMapper.DomainModel
{
    public class DicomTag : IValueObject, IEquatable<DicomTag>
    {
        public ushort Group { get; private set; }
        public ushort Element { get; private set; }

        public DicomTag(ushort group, ushort element)
        {
            Group = group;
            Element = element;
        }

        public DicomTag(string strTag)
        {
            Group = 0;
            Element = 0;
        }

        public static DicomTag PATIENTID = new DicomTag(0x0010, 0x0010);
        public static DicomTag MODALITY = new DicomTag(0x0010, 0x0010);
        public static DicomTag SOPINSTANCEUID= new DicomTag(0x0010, 0x0010);
        public static DicomTag SERIESINSTANCEUID = new DicomTag(0x0010, 0x0010);
        public static DicomTag ACQUISITIONDATETIME = new DicomTag(0x0010, 0x0010);

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