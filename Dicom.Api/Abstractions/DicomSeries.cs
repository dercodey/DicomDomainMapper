using System;

namespace Dicom.Api
{
    public class DicomSeries
    {
        public string SeriesUid { get; set; }

        public string StudyUid { get; set; }

        public string PatientName { get; set; }

        public string PatientId { get; set; }

        public string Modality { get; set; }

        public int ExpectedInstanceCount { get; set; }
    }
}
