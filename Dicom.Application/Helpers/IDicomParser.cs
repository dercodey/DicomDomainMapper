using System;
using System.Collections.Generic;
using System.IO;
using DomainModel = Elekta.Capability.Dicom.Domain.Model;

namespace Elekta.Capability.Dicom.Application.Helpers
{
    /// <summary>
    /// IDicomParser is a generic interface to some kind of parser engine
    /// </summary>
    public interface IDicomParser
    {
        /// <summary>
        /// ParseStream is the only entry point to an IDicomParser, that returns a collection of DicomAttributes
        /// </summary>
        /// <param name="dicomStream"></param>
        /// <returns></returns>
        IEnumerable<DomainModel.DicomAttribute> ParseStream(Stream dicomStream);
    }
}
