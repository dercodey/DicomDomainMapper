using System;
using System.Collections.Generic;
using System.IO;
using DomainModel = Dicom.Domain.Model;

namespace Dicom.Application.Helpers
{
    public interface IDicomParser
    {
        IEnumerable<DomainModel.DicomElement> ParseStream(Stream dicomStream);
    }
}
