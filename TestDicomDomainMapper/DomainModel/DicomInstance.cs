using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestDicomDomainMapper.DomainModel
{
    class DicomInstance : IEntity<DicomUid>
    {
        public DicomUid EntityId => SopInstanceUid;

        [IgnoreMap]
        public DicomUid SopInstanceUid { get; private set; }

        public IEnumerable<DicomAttribute> DicomAttributes 
        { 
            get;
            private set;
        }

        public DicomInstance(DicomUid sopInstanceUid, IEnumerable<DicomAttribute> dicomAttributes)
        {
            SopInstanceUid = sopInstanceUid;
            DicomAttributes = dicomAttributes;
        }
    }
}
