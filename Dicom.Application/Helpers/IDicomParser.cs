using System;
using System.Collections.Generic;
using System.IO;
using DomainModel = Elekta.Capability.Dicom.Domain.Model;

namespace Elekta.Capability.Dicom.Application.Helpers
{
    public interface IDicomParser
    {
        IEnumerable<DomainModel.DicomAttribute> ParseStream(Stream dicomStream);
    }
}
