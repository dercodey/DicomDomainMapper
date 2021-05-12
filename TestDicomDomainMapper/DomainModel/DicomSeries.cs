using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestDicomDomainMapper.DomainModel
{
    /// <summary>
    /// aggregate entity for a dicom series
    /// </summary>
    public class DicomSeries : Seedworks.IAggregateRoot<DicomUid>
    {
        /// <summary>
        /// construct a series entity
        /// </summary>
        /// <param name="seriesInstanceUid">UID identifying the series</param>
        /// <param name="patientId">patient ID that the series belongs to</param>
        /// <param name="modality">series modality</param>
        /// <param name="acquisitionDateTime">when did the series get acquired</param>
        /// <param name="dicomInstances">collection of initial dicom instances</param>
        public DicomSeries(DicomUid seriesInstanceUid, string patientId, string modality, 
            DateTime acquisitionDateTime, 
            IEnumerable<DicomInstance> dicomInstances)
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
        /// aggregate root key is the series instance UID
        /// </summary>
        public DicomUid RootKey => SeriesInstanceUid;

        /// <summary>
        /// this represents the key for the series in the domain model
        /// </summary>
        [IgnoreMap]
        public DicomUid SeriesInstanceUid 
        { 
            get; 
            private set; 
        }

        /// <summary>
        /// patient ID to whom the series belongs
        /// </summary>
        public string PatientId 
        { 
            get; 
            private set; 
        }

        /// <summary>
        /// series modality
        /// </summary>
        public string Modality 
        { 
            get; 
            private set; 
        }

        /// <summary>
        /// acquisition date/time for the series
        /// </summary>
        public DateTime AcquisitionDateTime 
        { 
            get; 
            private set; 
        }

        /// <summary>
        /// collection of DICOM instances that belong to the series
        /// </summary>
        [IgnoreMap]
        public List<DicomInstance> DicomInstances 
        { 
            get { return _instances; }
            private set { _instances = value.ToList(); }
        }

        private List<DicomInstance> _instances = new List<DicomInstance>();

        /// <summary>
        /// adds a new instance to the series, if it is consistant
        /// </summary>
        /// <param name="newInstance">the new instance to be added</param>
        /// <returns>true if successful</returns>
        public bool AddInstance(DicomInstance newInstance)
        {
            // perform checks by extracting relevent attributes
            var patientIdAttribute =
                newInstance.DicomAttributes.Single(attribute => attribute.DicomTag.Equals(DicomTag.PATIENTID));
            if (patientIdAttribute.Value.CompareTo(PatientId.ToString()) != 0)
            {
                throw new ArgumentException();
            }

            // perform checks by extracting relevent attributes
            var modalityAttribute = 
                newInstance.DicomAttributes.Single(attribute => attribute.DicomTag.Equals(DicomTag.MODALITY));
            if (modalityAttribute.Value.CompareTo(Modality) != 0)
            {
                throw new ArgumentException();
            }

#if CHECK_DATETIME
            var acquisitionDateTimeAttribute =
                newInstance.DicomAttributes.Single(attribute => attribute.DicomTag.Equals(DicomTag.ACQUISITIONDATETIME));
            if (modalityAttribute.Value.CompareTo(AcquisitionDateTime.ToString()) != 0)
            {
                throw new ArgumentException();
            }
#endif

            // everything is OK, so add to the collection
            _instances.Add(newInstance);

            // TODO: send an event that the instance was added

            return true;
        }
    }
}
