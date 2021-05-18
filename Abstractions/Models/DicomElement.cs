using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elekta.Capability.Dicom.Abstractions.Models
{
    public class DicomElement
    {
        public string DicomTag { get; set; }

        public string Value { get; set; }
    }
}
