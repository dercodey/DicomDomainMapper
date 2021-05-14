using System;
using System.Threading.Tasks;
using DomainModel = Dicom.Domain.Model;

namespace Dicom.Application.Services
{
    public interface IDicomApplicationService
    {
        DomainModel.DicomSeries GetAllSeries();

        DomainModel.DicomSeries GetSeriesByUid(DomainModel.DicomUid seriesInstanceUid);

        Task CreateSeriesAsync(string patientName, string patientId, 
            string seriesInstanceUid, string modality, int expectedInstanceCount, DateTime acquisitionDateTime);

        Task AddInstanceAsync(string seriesInstanceUid, string modality, DateTime acqusitionDateTime, string sopInstanceUid);

        Task ReconcilePatientName(string seriesInstanceUid, string oldPatientName, string newPatientName);
    }
}
