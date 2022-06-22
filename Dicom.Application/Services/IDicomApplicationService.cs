using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DomainModel = Elektrum.Capability.Dicom.Domain.Model;

namespace Elektrum.Capability.Dicom.Application.Services
{
    /// <summary>
    /// service interface for the DICOM application service
    /// </summary>
    public interface IDicomApplicationService
    {
        /// <summary>
        /// returns all series stored for the given patient
        /// </summary>
        /// <param name="patientId"></param>
        /// <returns></returns>
        IEnumerable<DomainModel.DicomSeries> GetAllSeriesForPatient(string patientId);

        /// <summary>
        /// retrieves a series by key (i.e. DicomUid)
        /// </summary>
        /// <param name="seriesInstanceUid">the series instance UID to be retrieved</param>
        /// <returns>the DicomSeries</returns>
        DomainModel.DicomSeries GetSeriesByUid(string seriesInstanceUid);

        /// <summary>
        /// creates a new series
        /// </summary>
        /// <param name="dmNewSeries"></param>
        /// <returns>task representing status of operation</returns>
        Task CreateSeriesAsync(DomainModel.DicomSeries dmNewSeries);

        /// <summary>
        /// add a dicom instance from a stream
        /// </summary>
        /// <param name="seriesInstanceUid"></param>
        /// <param name="readStream"></param>
        /// <returns>the Dicom UID of the series that the instance was added to</returns>
        Task<DomainModel.DicomUid> AddInstanceFromStreamAsync(string seriesInstanceUid, Stream readStream);

        /// <summary>
        /// perform a reconciliation of the instances in the series
        /// </summary>
        /// <param name="seriesInstanceUid"></param>
        /// <param name="oldPatientName"></param>
        /// <param name="newPatientName"></param>
        /// <returns>task representing status of the asynchronous operation</returns>
        Task ReconcilePatientNameAsync(string seriesInstanceUid, string oldPatientName, string newPatientName);

        /// <summary>
        /// retrieve a dicom instance
        /// </summary>
        /// <param name="seriesInstanceUid"></param>
        /// <param name="sopInstanceUid"></param>
        /// <returns>task representing status of the asynchronous operation</returns>
        Task<DomainModel.DicomInstance> GetDicomInstanceAsync(string seriesInstanceUid, string sopInstanceUid);

        /// <summary>
        /// delete the series
        /// </summary>
        /// <param name="seriesInstanceUid"></param>
        /// <returns>task representing status of the asynchronous operation</returns>
        Task DeleteDicomSeriesAsync(string seriesInstanceUid);
    }
}
