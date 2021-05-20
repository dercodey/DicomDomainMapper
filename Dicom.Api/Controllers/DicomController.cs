using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DomainModel = Elekta.Capability.Dicom.Domain.Model;
using AbstractionModel = Elekta.Capability.Dicom.Abstractions.Models;
using Elekta.Capability.Dicom.Application.Services;
using AutoMapper;

namespace Elekta.Capability.Dicom.Api.Controllers
{
    /// <summary>
    /// controller to handle DICOM requests
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class DicomController : ControllerBase
    {
        private readonly IDicomApplicationService _applicationService;
        private readonly IMapper _mapper;
        private readonly ILogger<DicomController> _logger;

        /// <summary>
        /// construct by injecting the application service and a logger
        /// </summary>
        /// <param name="applicationService"></param>
        /// <param name="logger"></param>
        public DicomController(IDicomApplicationService applicationService, IMapper mapper, ILogger<DicomController> logger)
        {
            _applicationService = applicationService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="patientId"></param>
        /// <returns></returns>
        [HttpGet("patient/{patientId}/series")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        public ActionResult<IEnumerable<AbstractionModel.DicomSeries>> GetAllDicomSeriesForPatient(string patientId)
        {
            try
            {
                // get all of the serieses for the patient
                var dmAllDicomSeries = _applicationService.GetAllSeriesForPatient(patientId);
                _logger.LogDebug($"Retrieved {dmAllDicomSeries.Count()} series for patient {patientId}");

                // convert to abstraction series
                var abAllDicomSeries = dmAllDicomSeries.Select(_mapper.Map<AbstractionModel.DicomSeries>);

                // and return the result
                return Ok(abAllDicomSeries);
            }
            catch (ArgumentException ex)
            {
                // couldn't find the patient
                _logger.LogError(ex.Message);
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="patientId"></param>
        /// <param name="seriesInstanceUid"></param>
        /// <returns></returns>
        [HttpGet("patient/{patientId}/series/{seriesInstanceUid}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        public ActionResult<AbstractionModel.DicomSeries> GetDicomSeries(string patientId, string seriesInstanceUid)
        {
            try
            {
                // try to retrieve the series
                var dmDicomSeries = _applicationService.GetSeriesByUid(seriesInstanceUid);
                _logger.LogDebug($"Retrieved {dmDicomSeries} series for patient {patientId}");

                // check the patient IDs match
                if (!dmDicomSeries.PatientId.Equals(patientId))
                {
                    return BadRequest("patient ID must match with stored value");
                }

                // map to abstraction
                var abDicomSeries = _mapper.Map<AbstractionModel.DicomSeries>(dmDicomSeries);

                // and return the result
                return Ok(abDicomSeries);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex.Message);
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="patientId"></param>
        /// <param name="abDicomSeries"></param>
        /// <returns></returns>
        [HttpPost("patient/{patientId}/series")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Post))]
        public async Task<ActionResult> AddDicomSeries(string patientId, [FromBody] AbstractionModel.DicomSeries abDicomSeries)
        {
            try
            {
                _logger.LogDebug($"Adding series for patient {patientId}");

                var dmDicomSeries = _mapper.Map<DomainModel.DicomSeries>(abDicomSeries);
                await _applicationService.CreateSeriesAsync(dmDicomSeries);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seriesUid"></param>
        /// <returns></returns>
        [HttpDelete("series/{seriesInstanceUid}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Post))]
        public async Task<ActionResult> DeleteDicomSeries(string seriesInstanceUid)
        {
            try
            {
                await _applicationService.DeleteDicomSeriesAsync(seriesInstanceUid);
                _logger.LogDebug($"Deleted series instance {seriesInstanceUid}");
                return Ok();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// adds a dicom instance from a IFormFile
        /// </summary>
        /// <param name="seriesInstanceU">the series UID to be added to</param>
        /// <param name="dicomFile">the IFormFile representing the DICOM blob</param>
        /// <returns></returns>
        [HttpPost("series/{seriesUid}/instances")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Post))]
        public async Task<ActionResult> AddDicomInstance(string seriesInstanceUid, IFormFile dicomFile)
        {
            if (dicomFile == null)
            {
                var msg = "file is null or empty.";
                _logger.LogError(msg);
                return BadRequest(msg);
            }

            try
            {
                var readStream = dicomFile.OpenReadStream();
                var addedSopInstanceUid = await _applicationService.AddInstanceFromStreamAsync(seriesInstanceUid, readStream);
                if (addedSopInstanceUid != null)
                {
                    // mismatched series instance UID
                    return BadRequest("mismatched series instance UID");
                }

                return Ok(addedSopInstanceUid.ToString());
            }
            catch (Exception ex) when (ex.Message.EndsWith("already exists."))
            {
                _logger.LogError(ex.Message);
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// retrieves a dicom instance, with optional query parameter
        /// </summary>
        /// <param name="seriesInstanceUid"></param>/// 
        /// <param name="sopInstanceUid"></param>
        /// <returns></returns>
        [HttpGet("series/{seriesInstanceUid}/instances/{sopInstanceUid}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        public async Task<ActionResult> GetDicomInstance(string seriesInstanceUid, string sopInstanceUid)
        {
            if (string.IsNullOrWhiteSpace(sopInstanceUid))
            {
                var msg = "id is null or empty.";
                _logger.LogError(msg);
                return BadRequest(msg);
            }

            try
            {
                // get the DICOM instance domain model
                var dmDicomInstance = 
                    await _applicationService.GetDicomInstanceAsync(seriesInstanceUid, sopInstanceUid);
                _logger.LogDebug($"Retrieved instance for {sopInstanceUid}");

                // map to the abstraction model
                var abDicomInstance = _mapper.Map<AbstractionModel.DicomInstance>(dmDicomInstance);

                // is there a query to be used?
                if (Request.Query != null
                    && Request.Query.ContainsKey("query"))
                {
                    // yes, so select the attributes that match the query
                    var attributes = Request.Query["query"].ToString().Split("+").ToList();
                    var filteredAttributes =
                        abDicomInstance.DicomAttributes.Where(attribute =>
                            attributes.Contains(attribute.DicomTag));

                    // and assign to the abstraction model
                    abDicomInstance.DicomAttributes = filteredAttributes.ToList();
                    _logger.LogDebug($"Filtered to {abDicomInstance.DicomAttributes.Count()}");
                }

                return Ok(abDicomInstance);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
}
