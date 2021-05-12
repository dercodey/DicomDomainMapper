using System;
using System.Diagnostics.CodeAnalysis;

namespace TestDicomDomainMapper.DomainModel
{
    /// <summary>
    /// value type for a Dicom Tag
    /// </summary>
    public class DicomTag : Seedworks.IValueObject, IEquatable<DicomTag>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        /// <param name="element"></param>
        public DicomTag(ushort group, ushort element)
        {
            Group = group;
            Element = element;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strTag"></param>
        public DicomTag(string strTag)
        {
            // TODO: parse these
            Group = 0;
            Element = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        public ushort Group 
        { 
            get; 
            private set; 
        }

        /// <summary>
        /// 
        /// </summary>
        public ushort Element 
        { 
            get; 
            private set; 
        }

        #region IEquatable
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals([AllowNull] DicomTag other)
        {
            return Group == other.Group && Element == other.Element;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Group:X2}{Element:X2}";
        }


        public static DicomTag PATIENTID = new DicomTag(0x0010, 0x0010);
        public static DicomTag MODALITY = new DicomTag(0x0010, 0x0010);
        public static DicomTag SOPINSTANCEUID = new DicomTag(0x0010, 0x0010);
        public static DicomTag SERIESINSTANCEUID = new DicomTag(0x0010, 0x0010);
        public static DicomTag ACQUISITIONDATETIME = new DicomTag(0x0010, 0x0010);


    }
}