using System;
using System.Collections.Generic;
using System.Linq;
using Elektrum.Capability.Dicom.Domain.Seedwork;

namespace Elektrum.Capability.Dicom.Domain.Model
{
    /// <summary>
    /// aggregate entity for a dicom series
    /// </summary>
    public class DicomSeries : IAggregateRoot<DicomUid>
    {
        /// <summary>
        /// construct a series entity
        /// </summary>
        /// <param name="seriesInstanceUid">UID identifying the series</param>
        /// <param name="patientName">patient name that the series belongs to</param>/// 
        /// <param name="patientId">patient ID that the series belongs to</param>
        /// <param name="modality">series modality</param>
        /// <param name="acquisitionDateTime">when did the series get acquired</param>
        /// <param name="expectedInstanceCount"></param>
        /// <param name="dicomInstances">collection of initial dicom instances</param>
        public DicomSeries(DicomUid seriesInstanceUid, 
            string patientName, string patientId, Modality modality, 
            DateTime acquisitionDateTime, 
            int expectedInstanceCount,
            IEnumerable<DicomInstance> dicomInstances = null)
        {
            if (seriesInstanceUid == null)
            {
                throw new ArgumentNullException("A Series Instance UID must be provided");
            }

            if (string.IsNullOrWhiteSpace(patientId))
            {
                throw new ArgumentException("PatientID must be a non-empty string");
            }

            if (acquisitionDateTime == null)
            {
                throw new ArgumentNullException("Series Acquistion DateTime must be provided");
            }

            SeriesInstanceUid = seriesInstanceUid;
            PatientName = patientName;
            PatientId = patientId;
            Modality = modality;
            AcquisitionDateTime = acquisitionDateTime;
            ExpectedInstanceCount = expectedInstanceCount;
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
        public DicomUid SeriesInstanceUid 
        { 
            get; 
            private set; 
        }

        /// <summary>
        /// patient name to whom the series belongs
        /// </summary>
        public string PatientName
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
        public Modality Modality 
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
        /// 
        /// </summary>
        public int ExpectedInstanceCount
        {
            get;
            private set;
        }

        /// <summary>
        /// current state of the series
        /// </summary>
        public SeriesState CurrentState
        {
            get
            {
                if (_instances == null
                    || _instances.Count == 0)
                {
                    return SeriesState.Created;
                }

                if (_instances.Count < ExpectedInstanceCount)
                {
                    return SeriesState.Incomplete;
                }
                else if (_instances.Count > ExpectedInstanceCount)
                {
                    return SeriesState.TooManyInstances;
                }

                return SeriesState.Complete;
            }
        }

        /// <summary>
        /// collection of DICOM instances that belong to the series
        /// </summary>
        public IEnumerable<DicomInstance> DicomInstances 
        { 
            get { return _instances; }
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
                throw new ArgumentException("DICOM instance doesn't match series PatientID");
            }

            // perform checks by extracting relevent attributes
            var modalityAttribute = 
                newInstance.DicomAttributes.Single(attribute => attribute.DicomTag.Equals(DicomTag.MODALITY));
            if (modalityAttribute.Value.CompareTo(Modality.ToString()) != 0)
            {
                throw new ArgumentException("DICOM instance doesn't match series modality");
            }

            var acquisitionDateTimeAttribute =
                newInstance.DicomAttributes.SingleOrDefault(attribute => attribute.DicomTag.Equals(DicomTag.ACQUISITIONDATETIME));
            if (acquisitionDateTimeAttribute != null)
            {
                var acquisitionDateTime = DateTime.Parse(acquisitionDateTimeAttribute.Value);
                if (acquisitionDateTime.CompareTo(AcquisitionDateTime) != 0)
                {
                    throw new ArgumentException("DICOM instance doesn't match series acquistion date time");
                }
            }

            // everything is OK, so add to the collection
            _instances.Add(newInstance);

            // TODO: send an event that the instance was added

            return true;
        }

        /// <summary>
        /// reconciles a series by updating the patient name
        /// </summary>
        /// <param name="oldPatientName"></param>
        /// <param name="newPatientName"></param>
        /// <returns></returns>
        public bool ReconcilePatientName(string oldPatientName, string newPatientName)
        {
            if (CurrentState != SeriesState.Complete)
            {
                throw new InvalidOperationException("Can not reconcile patient for incomplete series");
            }

            var updatedInstances =
                _instances.Select(instance =>
                    new DicomInstance(instance.SopInstanceUid,
                        instance.DicomAttributes.Select(attribute =>
                            attribute.DicomTag.Equals(DicomTag.PATIENTNAME)
                                ? new DicomAttribute(DicomTag.PATIENTNAME, newPatientName)
                                : attribute)));

            _instances = updatedInstances.ToList();

            return true;
        }
    }
}
