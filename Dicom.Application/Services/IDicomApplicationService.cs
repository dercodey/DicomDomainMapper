using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DomainModel = Dicom.Domain.Model;

namespace Dicom.Application.Services
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
        DomainModel.DicomSeries GetSeriesByUid(DomainModel.DicomUid seriesInstanceUid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newSeries"></param>
        /// <returns></returns>
        Task CreateSeriesAsync(DomainModel.DicomSeries newSeries);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="readStream"></param>
        /// <returns></returns>
        Task AddInstanceFromStreamAsync(Stream readStream);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seriesInstanceUid"></param>
        /// <param name="oldPatientName"></param>
        /// <param name="newPatientName"></param>
        /// <returns></returns>
        Task ReconcilePatientNameAsync(DomainModel.DicomUid seriesInstanceUid, string oldPatientName, string newPatientName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instanceUid"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        Task<DomainModel.DicomInstance> GetDicomInstanceAsync(DomainModel.DicomUid seriesInstanceUid, 
            DomainModel.DicomUid instanceUid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seriesUid"></param>
        /// <returns></returns>
        Task DeleteDicomSeriesAsync(DomainModel.DicomUid seriesUid);
    }
}
