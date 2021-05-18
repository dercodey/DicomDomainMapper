using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using Dicom.Application.Services;
using AbstractionModel = Elekta.Capability.Dicom.Abstractions.Models;

namespace Dicom.Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class DicomController : ControllerBase
    {
        private readonly IDicomApplicationService _applicationService;
        private readonly ILogger<DicomController> _logger;

        /// <summary>
        /// 
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
                    new Domain.Model.DicomUid(seriesInstanceUid));

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
                var mapper = Mappers.AbstractionMapper.GetMapper();
                var seriesDomainModel = mapper.Map<Domain.Model.DicomSeries>(dicomSeries);
                await _applicationService.CreateSeriesAsync(seriesDomainModel);
                return Ok();
            }
            catch (ArgumentException ex)
            {
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
                    new Domain.Model.DicomUid(seriesInstanceUid));
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seriesUid"></param>
        /// <param name="instance"></param>
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
                return BadRequest("file is null or empty.");
            }

            try
            {
                await _applicationService.AddInstanceFromStreamAsync(readStream);
                return Ok();
            }
            catch (Exception ex) when (ex.Message.EndsWith("already exists."))
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
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
                return BadRequest("id is null or empty.");
            }

            try
            {
                var dmDicomInstance = 
                    await _applicationService.GetDicomInstanceAsync(
                        new Domain.Model.DicomUid(seriesInstanceUid),
                        new Domain.Model.DicomUid(sopInstanceUid));

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
                return BadRequest(ex.Message);
            }
        }
    }
}
