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

        public DicomController(IDicomApplicationService application, ILogger<DicomController> logger)
        {
            _logger = logger;
        }

        [HttpGet("series")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        public IEnumerable<DicomSeries> GetAllSeries()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new DicomSeries
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
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        public IEnumerable<DicomSeries> GetSeries(string seriesInstanceUid)
        {
            var rng = new Random();
            return Enumerable.Range(1, 1).Select(index => new DicomSeries
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
    }
}
