using System;
using System.Collections.Generic;
using System.Text;

namespace TestDicomDomainMapper.DomainModel
{
    class DicomInstance : IEntity<DicomUid>
    {
        public DicomUid EntityId => SopInstanceUid;

        public DicomUid SopInstanceUid { get; private set; }

        public IEnumerable<DicomAttribute> DicomAttributes { get; private set; }

        public static DicomInstance Create(DicomUid sopInstanceUid, IEnumerable<DicomAttribute> attributes)
        {
            return new DicomInstance()
            {
                SopInstanceUid = sopInstanceUid,
                DicomAttributes = attributes,
            };
        }
    }
}
