using System.Collections.Generic;

namespace Elekta.Capability.Dicom.Abstractions.Models
{
    public class DicomInstance
    {
        public string SopInstanceUid { get; set; }

        public IEnumerable<DicomElement> DicomElements { get; set; }
    }
}
