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
        /// adds a dicom instance from a IFormFile
        /// </summary>
        /// <param name="seriesUid">the series UID to be added to</param>
        /// <param name="dicomFile">the IFormFile representing the DICOM blob</param>
        /// <returns></returns>
        [HttpPost("series/{seriesUid}/instances")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Post))]
        public async Task<ActionResult> AddDicomInstance(string seriesUid, IFormFile dicomFile)
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
                var seriesInstanceUid = await _applicationService.AddInstanceFromStreamAsync(readStream);
                if (!seriesInstanceUid.ToString().Equals(seriesUid))
                {
                    // mismatched series instance UID?  should we be adding in this case?
                    return BadRequest();
                }

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
                    await _applicationService.GetDicomInstanceAsync(
                        new DomainModel.DicomUid(seriesInstanceUid),
                        new DomainModel.DicomUid(sopInstanceUid));

                // map to the abstraction model
                var mapper = Mappers.AbstractionMapper.GetMapper();
                var abDicomInstance = mapper.Map<AbstractionModel.DicomInstance>(dmDicomInstance);

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
