using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Dicom.Api.Abstractions
{
    public class DicomInstance
    {
        public string SopInstanceUid { get; set; }

        public IEnumerable<DicomElement> DicomAttributes { get; set; }
    }
}
