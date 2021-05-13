using System;
using System.Collections.Generic;
using System.Text;

namespace Dicom.Application.Services
{
    public class DicomApplicationService : IDicomApplicationService
    {
        public void CreateSeries(string seriesInstanceUid, string modality, DateTime acquisitionDateTime)
        {
            throw new NotImplementedException();
        }

        public void AddInstance(string seriesInstanceUid, string modality, DateTime acquisitionDateTime, string sopInstanceUid)
        {
            throw new NotImplementedException();
        }
    }
}
