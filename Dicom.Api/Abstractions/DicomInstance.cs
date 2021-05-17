using Microsoft.AspNetCore.Http;

namespace Dicom.Api.Abstractions
{
    public class DicomInstance
    {
        public IFormFile File { get; set; }
    }
}
