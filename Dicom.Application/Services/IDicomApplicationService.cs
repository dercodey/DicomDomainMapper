using System;
using System.Threading.Tasks;

namespace Dicom.Application.Services
{
    public interface IDicomApplicationService
    {
        Task CreateSeriesAsync(string patientName, string patientId, 
            string seriesInstanceUid, string modality, int expectedInstanceCount, DateTime acquisitionDateTime);

        Task AddInstanceAsync(string seriesInstanceUid, string modality, DateTime acqusitionDateTime, string sopInstanceUid);

        Task ReconcilePatientName(string seriesInstanceUid, string oldPatientName, string newPatientName);
    }
}
