using System;
using System.Collections.Generic;
using System.IO;
using DomainModel = Elekta.Capability.Dicom.Domain.Model;

namespace Elekta.Capability.Dicom.Application.Helpers
{
    public interface IDicomParser
    {
        IEnumerable<DomainModel.DicomElement> ParseStream(Stream dicomStream);
    }
}
