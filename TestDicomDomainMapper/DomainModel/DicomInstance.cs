using System;
using System.Collections.Generic;
using System.Text;

namespace TestDicomDomainMapper.DomainModel
{
    class DicomInstance : IEntity<string>
    {
        public string EntityId => SopInstanceUid;

        public string SopInstanceUid { get; set; }

        public IEnumerable<DicomAttribute> DicomAttributes 
        { 
            get; 
            set; 
        }

        public static DicomInstance Create(string sopInstanceUid, IEnumerable<DicomAttribute> attributes)
        {
            return new DicomInstance()
            {
                SopInstanceUid = sopInstanceUid,
                DicomAttributes = attributes,
            };
        }
    }
}
