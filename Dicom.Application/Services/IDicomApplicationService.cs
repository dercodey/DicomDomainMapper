using System;
using System.Threading.Tasks;

namespace Dicom.Application.Services
{
    public interface IDicomApplicationService
    {
        Task CreateSeriesAsync(string patientId, string seriesInstanceUid, string modality, DateTime acquisitionDateTime);

        Task AddInstanceAsync(string seriesInstanceUid, string modality, DateTime acqusitionDateTime, string sopInstanceUid);
    }
}
