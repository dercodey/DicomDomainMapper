using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using DomainModel = Elekta.Capability.Dicom.Domain.Model;
using AbstractionModel = Elekta.Capability.Dicom.Abstractions.Models;
using Elekta.Capability.Dicom.Application.Services;

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
        private readonly ILogger<DicomController> _logger;

        /// <summary>
        /// construct by injecting the application service and a logger
        /// </summary>
        /// <param name="applicationService"></param>
        /// <param name="logger"></param>
        public DicomController(IDicomApplicationService applicationService, ILogger<DicomController> logger)
        {
            _applicationService = applicationService;
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

                // convert to abstraction series
                var mapper = Mappers.AbstractionMapper.GetMapper();
                var abAllDicomSeries = dmAllDicomSeries.Select(mapper.Map<AbstractionModel.DicomSeries>);

                // and return the result
                return Ok(abAllDicomSeries);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex.Message);
                // couldn't find the patient
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
                var seriesDomainModel = _applicationService.GetSeriesByUid(
                    new DomainModel.DicomUid(seriesInstanceUid));

                // check the patient IDs match
                if (!seriesDomainModel.PatientId.Equals(patientId))
                {
                    return BadRequest("patient ID must match with stored value");
                }

                // map to abstraction
                var mapper = Mappers.AbstractionMapper.GetMapper();
                var seriesAbstraction = mapper.Map<AbstractionModel.DicomSeries>(seriesDomainModel);

                // and return the result
                return Ok(seriesAbstraction);
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
        /// <param name="dicomSeries"></param>
        /// <returns></returns>
        [HttpPost("patient/{patientId}/series")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Post))]
        public async Task<ActionResult> AddDicomSeries(string patientId, [FromBody] AbstractionModel.DicomSeries dicomSeries)
        {
            try
            {
                _logger.LogDebug($"Adding series for patient {patientId}");

                var mapper = Mappers.AbstractionMapper.GetMapper();
                var seriesDomainModel = mapper.Map<DomainModel.DicomSeries>(dicomSeries);
                await _applicationService.CreateSeriesAsync(seriesDomainModel);
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
                await _applicationService.DeleteDicomSeriesAsync(
                    new DomainModel.DicomUid(seriesInstanceUid));
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
        /// <param name="readStream"></param>
        /// <returns></returns>
        [HttpPost("series/{seriesUid}/instances")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Post))]
        public async Task<ActionResult> AddDicomInstance(string seriesUid, [FromForm] Stream readStream)
        {
            if (readStream == null)
            {
                var msg = "file is null or empty.";
                _logger.LogError(msg);
                return BadRequest(msg);
            }

            try
            {
                await _applicationService.AddInstanceFromStreamAsync(readStream);
                return Ok();
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
        /// 
        /// </summary>
        /// <param name="instanceUid"></param>
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
                var dmDicomInstance = 
                    await _applicationService.GetDicomInstanceAsync(
                        new DomainModel.DicomUid(seriesInstanceUid),
                        new DomainModel.DicomUid(sopInstanceUid));

                var mapper = Mappers.AbstractionMapper.GetMapper();
                var abDicomInstance = mapper.Map<AbstractionModel.DicomInstance>(dmDicomInstance);

                if (Request.Query.ContainsKey("query"))
                {
                    var attributes = Request.Query["query"].ToString().Split(",").ToList();
                    var filteredAttributes =
                        abDicomInstance.DicomAttributes.Where(attribute =>
                            attributes.Contains(attribute.DicomTag.ToString()));

                    abDicomInstance.DicomAttributes = filteredAttributes;
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
