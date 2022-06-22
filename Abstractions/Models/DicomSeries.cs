using System;

namespace Elektrum.Capability.Dicom.Abstractions.Models
{
    public class DicomSeries
    {
        public string SeriesInstanceUid { get; set; }

        public string PatientName { get; set; }

        public string PatientId { get; set; }

        public string Modality { get; set; }

        public DateTime AcquisitionDateTime { get; set; }

        public int ExpectedInstanceCount { get; set; }
    }
}
