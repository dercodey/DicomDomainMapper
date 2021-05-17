using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DomainModel = Dicom.Domain.Model;

namespace Dicom.Application.Services
{
    public interface IDicomApplicationService
    {
        DomainModel.DicomSeries GetAllSeries();

        DomainModel.DicomSeries GetSeriesByUid(DomainModel.DicomUid seriesInstanceUid);

        Task CreateSeriesAsync(DomainModel.DicomSeries newSeries);

        Task AddInstanceFromStreamAsync(Stream readStream);

        Task AddInstanceAsync(string seriesInstanceUid, string modality, DateTime acqusitionDateTime, string sopInstanceUid);

        Task ReconcilePatientName(string seriesInstanceUid, string oldPatientName, string newPatientName);

        Task<IEnumerable<DomainModel.DicomAttribute>> GetAttributesAsync(string instanceUid, List<string> attributes);
        Task DeleteDicomSeries(string seriesUid);
    }
}
