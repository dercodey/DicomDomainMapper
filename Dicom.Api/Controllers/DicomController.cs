﻿using Microsoft.AspNetCore.Mvc;
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
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        public IEnumerable<DicomSeries> GetAllDicomSeries()
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
        public ActionResult<DicomSeries> GetDicomSeries(string seriesInstanceUid)
        {
            try
            {
                var seriesInstanceDicomUid = new Domain.Model.DicomUid(seriesInstanceUid);
                var seriesDomainModel = _application.GetSeriesByUid(seriesInstanceDicomUid);
                var seriesAbstraction =
                    new DicomSeries()
                    {
                        PatientId = seriesDomainModel.PatientId,
                        PatientName = seriesDomainModel.PatientName,
                        StudyUid = $"1.2.3.9",
                        SeriesUid = seriesDomainModel.SeriesInstanceUid.ToString(),
                        Modality = seriesDomainModel.Modality,
                        ExpectedInstanceCount = seriesDomainModel.ExpectedInstanceCount,
                    };
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
        public async Task<ActionResult> CreateDicomSeries([FromBody] DicomSeries dicomSeries)
        {
            var seriesInstanceDicomUid = new Domain.Model.DicomUid(dicomSeries.SeriesUid);

            var seriesDomainModel = 
                new Domain.Model.DicomSeries(
                    seriesInstanceDicomUid, 
                    dicomSeries.PatientName, dicomSeries.PatientId, dicomSeries.Modality, 
                    DateTime.Now, dicomSeries.ExpectedInstanceCount, null);

            await _application.CreateSeriesAsync(seriesDomainModel);

            return Ok();
        }
    }
}
