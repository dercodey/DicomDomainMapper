using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dicom.Application.Repositories;
using DomainModel = Dicom.Domain.Model;

namespace Dicom.Application.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class DicomApplicationService : IDicomApplicationService
    {
        private readonly IAggregateRepository<DomainModel.DicomSeries, DomainModel.DicomUid> _repository;

        public DicomApplicationService(IAggregateRepository<DomainModel.DicomSeries, DomainModel.DicomUid> repository)
        {
            _repository = repository;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="patientId"></param>
        /// <returns></returns>
        public IEnumerable<DomainModel.DicomSeries> GetAllSeriesForPatient(string patientId)
        {
            var allSeriesForPatient = _repository.SelectAggregates(series => series.PatientId.Equals(patientId));
            return allSeriesForPatient;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seriesInstanceUid"></param>
        /// <returns></returns>
        public DomainModel.DicomSeries GetSeriesByUid(DomainModel.DicomUid seriesInstanceUid)
        {
            var seriesDomainModel = _repository.GetAggregateForKey(seriesInstanceUid);
            return seriesDomainModel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="forSeries"></param>
        /// <returns></returns>
        public async Task CreateSeriesAsync(DomainModel.DicomSeries forSeries)
        {
            var existingSeries = _repository.GetAggregateForKey(forSeries.SeriesInstanceUid);
            if (existingSeries != null)
            {
                throw new ArgumentException();
            }

            await _repository.UpdateAsync(forSeries);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="readStream"></param>
        /// <returns></returns>
        public async Task AddInstanceFromStreamAsync(Stream readStream)
        {
            var dicomParser = new Kaitai.Dicom(new Kaitai.KaitaiStream(readStream));

            var selectedElements =
                dicomParser.Elements.Select(element =>
                {
                    var dmTag = DomainModel.DicomTag.GetTag($"({element.TagGroup:X4}, {element.TagElem:X4})");
                    if (dmTag == null)
                        return null;

                    var value = Encoding.UTF8.GetString(element.Value);
                    return new DomainModel.DicomElement(dmTag, value);
                })
                .Where(element => element != null);

            var sopInstanceUid = selectedElements.Single(da => da.DicomTag.Equals(DomainModel.DicomTag.SOPINSTANCEUID));

            var dmDicomInstance =
                new DomainModel.DicomInstance(new DomainModel.DicomUid(sopInstanceUid.Value), selectedElements);

            var seriesInstanceUid = selectedElements.Single(da => da.DicomTag.Equals(DomainModel.DicomTag.SOPINSTANCEUID));
            var existingSeries = _repository.GetAggregateForKey(new DomainModel.DicomUid(seriesInstanceUid.Value));
            if (existingSeries == null)
            {
                throw new ArgumentException();
            }

            existingSeries.AddInstance(dmDicomInstance);

            await _repository.UpdateAsync(existingSeries);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seriesInstanceUid"></param>
        /// <param name="oldPatientName"></param>
        /// <param name="newPatientName"></param>
        /// <returns></returns>
        public Task ReconcilePatientNameAsync(DomainModel.DicomUid seriesInstanceUid, string oldPatientName, string newPatientName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seriesInstanceUid"></param>
        /// <param name="sopInstanceUid"></param>
        /// <returns></returns>
        public Task<DomainModel.DicomInstance> GetDicomInstanceAsync(DomainModel.DicomUid seriesInstanceUid,
            DomainModel.DicomUid sopInstanceUid)
        {
            var seriesDomainModel = GetSeriesByUid(seriesInstanceUid);

            var matchingInstance =
                seriesDomainModel.DicomInstances.Where(instance => 
                        instance.SopInstanceUid.Equals(sopInstanceUid))
                    .Single();

            return Task.FromResult(matchingInstance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seriesUid"></param>
        /// <returns></returns>
        public async Task DeleteDicomSeriesAsync(DomainModel.DicomUid seriesInstanceUid)
        {
            await _repository.RemoveAsync(seriesInstanceUid);
        }
    }
}
