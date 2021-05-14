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

        public async Task CreateSeriesAsync(string patientName, string patientId, string seriesInstanceUid, string modality, int expectedInstanceCount, DateTime acquisitionDateTime)
        {
            var seriesInstanceDicomUid = new DomainModel.DicomUid(seriesInstanceUid);
            var existingSeries = _repository.GetAggregateForKey(seriesInstanceDicomUid);
            if (existingSeries != null)
            {
                throw new ArgumentException();
            }

            var dicomSeries = new DomainModel.DicomSeries(seriesInstanceDicomUid, patientName, patientId, modality, acquisitionDateTime, expectedInstanceCount, null);
            await _repository.UpdateAsync(dicomSeries);
        }

        public async Task AddInstanceAsync(string seriesInstanceUid, string modality, DateTime acquisitionDateTime, string sopInstanceUid)
        {
            throw new NotImplementedException();
        }

        public Task ReconcilePatientName(string seriesInstanceUid, string oldPatientName, string newPatientName)
        {
            throw new NotImplementedException();
        }
    }
}
