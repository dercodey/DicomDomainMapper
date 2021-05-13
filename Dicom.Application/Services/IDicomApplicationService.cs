using System;

namespace Dicom.Application.Services
{
    public interface IDicomApplicationService
    {
        public void CreateSeries(string seriesInstanceUid, string modality, DateTime acquisitionDateTime);

        void AddInstance(string seriesInstanceUid, string modality, DateTime acqusitionDateTime, string sopInstanceUid);
    }
}
