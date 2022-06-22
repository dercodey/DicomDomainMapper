using System;
using System.Collections.Generic;

namespace Elektrum.Capability.Dicom.Infrastructure.EFModel
{
    public class DicomSeries
    {
        public int ID { get; set; }

        public string SeriesInstanceUid { get; set; }

        public string PatientName { get; set; }

        public string PatientID { get; set; }

        public DateTime AcquisitionDateTime { get; set; }

        public string Modality { get; set; }

        public int ExpectedInstanceCount { get; set; }

        public List<DicomInstance> DicomInstances { get; set; } = 
            new List<DicomInstance>();
    }
}
