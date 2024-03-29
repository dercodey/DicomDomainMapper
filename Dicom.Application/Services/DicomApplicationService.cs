﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DomainModel = Elektrum.Capability.Dicom.Domain.Model;
using Elektrum.Capability.Dicom.Application.Repositories;
using Elektrum.Capability.Dicom.Application.Helpers;
using Elektrum.Capability.Dicom.Application.Messaging;

namespace Elektrum.Capability.Dicom.Application.Services
{
    /// <summary>
    /// application service object for supporting calls in to the domain model
    /// </summary>
    public class DicomApplicationService : IDicomApplicationService
    {
        private readonly IAggregateRepository<DomainModel.DicomSeries, DomainModel.DicomUid> _repository;
        private readonly IMessaging _messaging;
        private readonly IDicomParser _dicomParser;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="messaging"></param>
        /// <param name="dicomParser"></param>
        /// <param name="logger"></param>
        public DicomApplicationService(IAggregateRepository<DomainModel.DicomSeries, DomainModel.DicomUid> repository, 
            IMessaging messaging, IDicomParser dicomParser, ILogger<DicomApplicationService> logger)
        {
            _repository = repository;
            _messaging = messaging;
            _dicomParser = dicomParser;
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
        public DomainModel.DicomSeries GetSeriesByUid(string seriesInstanceUid)
        {
            var dmSeriesInstanceUid = new DomainModel.DicomUid(seriesInstanceUid);

            var dmDicomSeries = _repository.GetAggregateForKey(dmSeriesInstanceUid);
            return dmDicomSeries;
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
        /// adds a DICOM instance represented as a stream
        /// </summary>
        /// <param name="seriesInstanceUid"></param>
        /// <param name="readStream"></param>
        /// <returns>the DICOM UID for the new instance, or null if not added</returns>
        public async Task<DomainModel.DicomUid> AddInstanceFromStreamAsync(string seriesInstanceUid, Stream readStream)
        {
            var parsedElements =
                _dicomParser.ParseStream(readStream);

            // check that the series has already been created
            var extractedSeriesInstanceUid = parsedElements.Single(da => da.DicomTag.Equals(DomainModel.DicomTag.SOPINSTANCEUID));
            if (extractedSeriesInstanceUid.Value != seriesInstanceUid)
            {
                return null;
            }

            var dmSeriesInstanceUid = new DomainModel.DicomUid(seriesInstanceUid);
            var existingSeries = _repository.GetAggregateForKey(dmSeriesInstanceUid);
            if (existingSeries == null)
            {
                throw new InvalidOperationException("SeriesInstanceUID not found in repository");
            }

            // create the new instance
            var sopInstanceUid = parsedElements.Single(da => 
                da.DicomTag.Equals(DomainModel.DicomTag.SOPINSTANCEUID));
            var dmSopInstanceUid = new DomainModel.DicomUid(sopInstanceUid.Value);
            var dmDicomInstance =
                new DomainModel.DicomInstance(dmSopInstanceUid, parsedElements);

            // now add the new instance to the series
            existingSeries.AddInstance(dmDicomInstance);

            // and commit the changes
            await _repository.UpdateAsync(existingSeries);

            if (existingSeries.CurrentState == DomainModel.SeriesState.Complete)
            {
                // send event
                await _messaging.SendNewSeriesEvent(existingSeries);
            }

            return dmSeriesInstanceUid;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seriesInstanceUid"></param>
        /// <param name="oldPatientName"></param>
        /// <param name="newPatientName"></param>
        /// <returns></returns>
        public Task ReconcilePatientNameAsync(string seriesInstanceUid, string oldPatientName, string newPatientName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seriesInstanceUid"></param>
        /// <param name="sopInstanceUid"></param>
        /// <returns></returns>
        public Task<DomainModel.DicomInstance> GetDicomInstanceAsync(string seriesInstanceUid, string sopInstanceUid)
        {
            var seriesDomainModel = GetSeriesByUid(seriesInstanceUid);

            var dmSopInstanceUid = new DomainModel.DicomUid(sopInstanceUid);
            var matchingInstance =
                seriesDomainModel.DicomInstances.Where(instance => 
                        instance.SopInstanceUid.Equals(dmSopInstanceUid))
                    .Single();

            return Task.FromResult(matchingInstance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seriesInstanceUid"></param>
        /// <returns></returns>
        public async Task DeleteDicomSeriesAsync(string seriesInstanceUid)
        {
            var dmSeriesInstanceUid = new DomainModel.DicomUid(seriesInstanceUid);
            await _repository.RemoveAsync(dmSeriesInstanceUid);
        }
    }
}
