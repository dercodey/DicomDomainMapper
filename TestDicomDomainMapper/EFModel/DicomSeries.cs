using AutoMapper;
using System;
using System.Collections.Generic;

namespace TestDicomDomainMapper.EFModel
{
    public class DicomSeries
    {
        [IgnoreMap]
        public int ID { get; set; }
        public string SeriesInstanceUid { get; set; }
        public string PatientID { get; set; }
        public DateTime AcquisitionDateTime { get; set; }
        public string Modality { get; set; }
        public List<DicomInstance> DicomInstances { get; set; }
    }
}
