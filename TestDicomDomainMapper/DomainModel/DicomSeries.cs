using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestDicomDomainMapper.DomainModel
{
    /// <summary>
    /// 
    /// </summary>
    public class DicomSeries : Seedworks.IAggregateRoot<DicomUid>
    {
        public DicomSeries(DicomUid seriesInstanceUid, string patientId, string modality, DateTime acquisitionDateTime, IEnumerable<DicomInstance> dicomInstances)
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

        /// <summary>
        /// 
        /// </summary>
        public DicomUid RootKey => SeriesInstanceUid;

        /// <summary>
        /// 
        /// </summary>
        [IgnoreMap]
        public DicomUid SeriesInstanceUid { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string PatientId { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string Modality { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime AcquisitionDateTime { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [IgnoreMap]
        public List<DicomInstance> DicomInstances 
        { 
            get { return _instances; }
            private set { _instances = value.ToList(); }
        }

        private List<DicomInstance> _instances = new List<DicomInstance>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newInstance"></param>
        /// <returns></returns>
        public bool AddInstance(DicomInstance newInstance)
        {
            // perform checks by extracting relevent attributes
            // ...

            _instances.Add(newInstance);

            return true;
        }
    }
}
