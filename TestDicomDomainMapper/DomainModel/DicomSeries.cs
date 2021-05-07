using System;
using System.Collections.Generic;
using System.Text;

namespace TestDicomDomainMapper.DomainModel
{
    class DicomSeries : IAggregateRoot<DicomUid>
    {
        public DicomUid RootId => SeriesInstanceUid;

        public DicomUid SeriesInstanceUid { get; private set; }
        public string PatientId { get; private set; }
        public string Modality { get; private set; }
        public DateTime AcquisitionDateTime { get; private set; }

        public IEnumerable<DicomInstance> Instances { get { return _instances; } }

        private List<DicomInstance> _instances;

        public bool AddInstance(DicomInstance newInstance)
        {
            // perform checks by extracting relevent attributes
            // ...

            _instances.Add(newInstance);

            return true;
        }

        public static DicomSeries Create(DicomUid seriesInstanceUid, string patientId, string modality, DateTime acquisitionDateTime, IEnumerable<DicomInstance> instances)
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
