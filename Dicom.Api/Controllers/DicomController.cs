using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dicom.Application.Services;

namespace Dicom.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DicomController : ControllerBase
    {
        private readonly IDicomApplicationService _applicationService;
        private readonly ILogger<DicomController> _logger;

        public DicomController(IDicomApplicationService applicationService, ILogger<DicomController> logger)
        {
            _applicationService = applicationService;
            _logger = logger;
        }

        [HttpGet("series")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        public IEnumerable<Abstractions.DicomSeries> GetAllDicomSeries()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new Abstractions.DicomSeries
            {
                PatientId = "98765",
                PatientName = "Last, First",
                SeriesInstanceUid = $"1.2.3.{index}",
                Modality = "CT",
                ExpectedInstanceCount = 3,
            })
            .ToArray();
        }

        [HttpGet("series/{seriesInstanceUid}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        public ActionResult<Abstractions.DicomSeries> GetDicomSeries(string seriesInstanceUid)
        {
            try
            {
                var seriesInstanceDicomUid = new Domain.Model.DicomUid(seriesInstanceUid);
                var seriesDomainModel = _applicationService.GetSeriesByUid(seriesInstanceDicomUid);

                var mapper = Mappers.AbstractionMapper.GetMapper();
                var seriesAbstraction = mapper.Map<Abstractions.DicomSeries>(seriesDomainModel);

                return Ok(seriesAbstraction);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        [HttpPost("series")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Post))]
        public async Task<ActionResult> AddDicomSeries([FromBody] Abstractions.DicomSeries dicomSeries)
        {
            try
            {
                var mapper = Mappers.AbstractionMapper.GetMapper();
                var seriesDomainModel = mapper.Map<Domain.Model.DicomSeries>(dicomSeries);
                await _applicationService.CreateSeriesAsync(seriesDomainModel);
                return Ok();
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }


        [HttpDelete("series/{seriesUid}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Post))]
        public async Task<ActionResult> DeleteDicomSeries(string seriesUid)
        {
            try
            {
                await _applicationService.DeleteDicomSeries(seriesUid);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("series/{seriesUid}/instances")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Post))]
        public async Task<ActionResult> AddDicomInstance(string seriesUid, [FromForm] Abstractions.DicomInstance instance)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("ModelState is invalid.");
            }

            if (instance == null)
            {
                return BadRequest("file is null or empty.");
            }

            try
            {
                var readStream = instance.File.OpenReadStream();
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

        [HttpGet("series/{seriesUid}/instances/{instanceUid}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        public async Task<ActionResult> GetDicomInstance(string instanceUid)
        {
            if (string.IsNullOrWhiteSpace(instanceUid))
            {
                return BadRequest("id is null or empty.");
            }

            if (Request.Query == null || Request.Query.Count == 0)
            {
                return BadRequest($"No query parameters specified.");
            }

            if (!Request.Query.ContainsKey("query"))
            {
                // TODO: or should this just return all attributes?
                return BadRequest($"Query does not contain key 'query'.");
            }

            try
            {
                var attributes = Request.Query["query"].ToString().Split(",").ToList();
                var dmDicomAttributes = await _applicationService.GetAttributesAsync(instanceUid, attributes);

                var mapper = Mappers.AbstractionMapper.GetMapper();
                var abDicomAttributes = mapper.Map<Abstractions.DicomAttribute>(dmDicomAttributes);
                return Ok(abDicomAttributes);
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }
    }
}
