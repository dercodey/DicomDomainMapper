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
        private readonly ILogger<DicomController> _logger;
        private readonly IDicomApplicationService _application;

        public DicomController(IDicomApplicationService application, ILogger<DicomController> logger)
        {
            _logger = logger;
            _application = application;
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
                StudyUid = $"1.2.3.9",
                SeriesUid = $"1.2.3.{index}",
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
                var seriesDomainModel = _application.GetSeriesByUid(seriesInstanceDicomUid);

                var mapper = Mappers.AbstractionMapper.GetMapper();
                var seriesAbstraction = mapper.Map<Abstractions.DicomSeries>(seriesDomainModel);

                return Ok(seriesAbstraction);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        [HttpPost("studies/{studyUid}/series")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Post))]
        public async Task<ActionResult> CreateDicomSeries([FromBody] Abstractions.DicomSeries dicomSeries)
        {
            var mapper = Mappers.AbstractionMapper.GetMapper();
            var seriesDomainModel = mapper.Map<Domain.Model.DicomSeries>(dicomSeries);

            await _application.CreateSeriesAsync(seriesDomainModel);

            return Ok();
        }
    }
}
