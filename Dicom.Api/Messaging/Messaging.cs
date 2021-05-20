using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using Elekta.Capability.Dicom.Domain.Model;
using AbstractionEvents = Elekta.Capability.Dicom.Abstractions.Events;
using Elekta.Capability.Dicom.Application.Messaging;

namespace Elekta.Capability.Dicom.Api.Messaging
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
