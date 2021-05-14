using System;
using System.Collections.Generic;
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
        public DomainModel.DicomSeries GetAllSeries()
        {
            throw new NotImplementedException();
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
    }
}
