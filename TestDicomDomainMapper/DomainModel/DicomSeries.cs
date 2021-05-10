using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestDicomDomainMapper.DomainModel
{
    class DicomSeries : IAggregateRoot<string>
    {
        public string RootId => SeriesInstanceUid;

        public string SeriesInstanceUid { get; set; }
        public string PatientId { get; set; }
        public string Modality { get; set; }
        public DateTime AcquisitionDateTime { get; set; }

        [IgnoreMap]
        public List<DicomInstance> DicomInstances 
        { 
            get { return _instances; } 
            set { _instances = value.ToList(); }
        }

        private List<DicomInstance> _instances = new List<DicomInstance>();

        public bool AddInstance(DicomInstance newInstance)
        {
            // perform checks by extracting relevent attributes
            // ...

            _instances.Add(newInstance);

            return true;
        }

        public static DicomSeries Create(string seriesInstanceUid, string patientId, string modality, DateTime acquisitionDateTime, IEnumerable<DicomInstance> instances)
        {
            var newSeries = new DicomSeries()
            {
                SeriesInstanceUid = seriesInstanceUid,
                PatientId = patientId,
                Modality = modality,
                AcquisitionDateTime = acquisitionDateTime,
            };
            newSeries._instances =
                instances != null 
                    ? new List<DicomInstance>(instances) 
                    : new List<DicomInstance>();
            return newSeries;
        }
    }
}
