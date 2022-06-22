using System;
using System.Collections.Generic;

namespace Elektrum.Capability.Dicom.Abstractions.Events
{
    public class DicomSeriesAddedEvent
    {
        public const string Subject = "DicomSeriesAddedEvent";

        public Guid MessageId { get; set; }

        public string SeriesInstanceUid { get; set; }

        public string PatientId { get; set; }

        public string Modality { get; set; }

        public IEnumerable<string> SopInstanceUids { get; set; }
    }
}
