using System;
using System.Collections.Generic;
using System.Text;

namespace TestDicomDomainMapper.DomainModel
{
    class DicomUid : IValueObject
    {
        public string UidString { get; private set; }

        public static DicomUid Create(string uidString)
        {
            // validate string is correct
            return new DicomUid { UidString = uidString };
        }
    }
}
