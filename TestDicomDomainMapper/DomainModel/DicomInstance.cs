using System;
using System.Collections.Generic;
using System.Text;

namespace TestDicomDomainMapper.DomainModel
{
    class DicomInstance : IEntity<string>
    {
        public string EntityId => SopInstanceUid;

        public string SopInstanceUid { get; private set; }

        public IEnumerable<DicomAttribute> DicomAttributes 
        { 
            get;
            private set;
        }

        public DicomInstance(string sopInstanceUid, IEnumerable<DicomAttribute> dicomAttributes)
        {
            SopInstanceUid = sopInstanceUid;
            DicomAttributes = dicomAttributes;
        }
    }
}
