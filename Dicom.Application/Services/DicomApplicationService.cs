using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Dicom.Application.Repositories;
using DomainModel = Dicom.Domain.Model;

namespace Dicom.Application.Services
{
    public class DicomApplicationService : IDicomApplicationService
    {
        private readonly IAggregateRepository<DomainModel.DicomSeries, DomainModel.DicomUid> _repository;

        public DicomApplicationService(IAggregateRepository<DomainModel.DicomSeries, DomainModel.DicomUid> repository)
        {
            _repository = repository;
        }
        public IEnumerable<DomainModel.DicomSeries> GetAllSeriesForPatient(string patientId)
        {
            var allSeriesForPatient = _repository.SelectAggregates(series => series.PatientId.Equals(patientId));
            return allSeriesForPatient;
        }

        public DomainModel.DicomSeries GetSeriesByUid(DomainModel.DicomUid seriesInstanceUid)
        {
            var seriesDomainModel = _repository.GetAggregateForKey(seriesInstanceUid);
            return seriesDomainModel;
        }

        public async Task CreateSeriesAsync(DomainModel.DicomSeries forSeries)
        {
            var existingSeries = _repository.GetAggregateForKey(forSeries.SeriesInstanceUid);
            if (existingSeries != null)
            {
                throw new ArgumentException();
            }

            await _repository.UpdateAsync(forSeries);
        }

        public Task AddInstanceAsync(string seriesInstanceUid, string modality, DateTime acquisitionDateTime, string sopInstanceUid)
        {
            throw new NotImplementedException();
        }

        public Task ReconcilePatientName(string seriesInstanceUid, string oldPatientName, string newPatientName)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<DomainModel.DicomAttribute>> GetAttributesAsync(string instanceUid, List<string> attributes)
        {
            throw new NotImplementedException();
        }

        public Task AddInstanceFromStreamAsync(Stream readStream)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteDicomSeries(string seriesUid)
        {
            await _repository.RemoveAsync(new DomainModel.DicomUid(seriesUid));
        }
    }
}
