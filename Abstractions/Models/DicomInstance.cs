using System.Collections.Generic;

namespace Elektrum.Capability.Dicom.Abstractions.Models
{
    public class DicomInstance
    {
        public string SopInstanceUid { get; set; }

        public IEnumerable<DicomAttribute> DicomAttributes { get; set; }
    }
}
