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

        public string SeriesInstanceUid { get; private set; }
        public string PatientId { get; private set; }
        public string Modality { get; private set; }
        public DateTime AcquisitionDateTime { get; private set; }

        [IgnoreMap]
        public List<DicomInstance> DicomInstances 
        { 
            get { return _instances; }
            private set { _instances = value.ToList(); }
        }

        private List<DicomInstance> _instances = new List<DicomInstance>();

        public bool AddInstance(DicomInstance newInstance)
        {
            // perform checks by extracting relevent attributes
            // ...

            _instances.Add(newInstance);

            return true;
        }

        public DicomSeries(string seriesInstanceUid, string patientId, string modality, DateTime acquisitionDateTime, IEnumerable<DicomInstance> dicomInstances)
        {
            SeriesInstanceUid = seriesInstanceUid;
            PatientId = patientId;
            Modality = modality;
            AcquisitionDateTime = acquisitionDateTime;
            _instances =
                dicomInstances != null 
                    ? new List<DicomInstance>(dicomInstances) 
                    : new List<DicomInstance>();
        }
    }
}
