using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using Elektrum.Capability.Dicom.Domain.Model;
using AbstractionEvents = Elektrum.Capability.Dicom.Abstractions.Events;
using Elektrum.Capability.Dicom.Application.Messaging;

namespace Elektrum.Capability.Dicom.Api.Messaging
{
    public class Messaging : IMessaging
    {
        private readonly ILogger<Messaging> _logger;

        public Messaging(ILogger<Messaging> logger)
        {
            _logger = logger;
        }

        public Task SendNewSeriesEvent(DicomSeries series)
        {
            var eventArgs = new AbstractionEvents.DicomSeriesAddedEvent
            {
                MessageId = Guid.NewGuid(),
                SeriesInstanceUid = series.SeriesInstanceUid.ToString(),
                PatientId = series.PatientId,
                SopInstanceUids = series.DicomInstances.Select(instance => 
                    instance.SopInstanceUid.ToString()),
                Modality = series.Modality.ToString(),
            };
            _logger.LogDebug($"Sent event for new series {eventArgs}");

            return Task.CompletedTask;
        }
    }
}
