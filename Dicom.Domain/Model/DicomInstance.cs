using System;
using System.Collections.Generic;
using System.Linq;
using Elekta.Capability.Dicom.Domain.Seedwork;

namespace Elekta.Capability.Dicom.Domain.Model
{
    /// <summary>
    /// entity representing a dicom instance
    /// </summary>
    public class DicomInstance : IEntity<DicomUid>
    {
        /// <summary>
        /// construct an instance with the given instance UID and the corresponding attributes
        /// </summary>
        /// <param name="sopInstanceUid"></param>
        /// <param name="dicomElements"></param>
        public DicomInstance(DicomUid sopInstanceUid, IEnumerable<DicomElement> dicomElements)
        {
            if (sopInstanceUid == null)
            {
                throw new ArgumentNullException("SOP Instance UID must be provided");
            }

            var embeddedSopInstanceUid = 
                dicomElements.SingleOrDefault(attribute => attribute.DicomTag.Equals(DicomTag.SOPINSTANCEUID));

            if (embeddedSopInstanceUid == null)
            {
                // the SOP instance UID is missing
                throw new ArgumentException("Missing SOP Instance UID in DICOM instance");
            }

            else if (embeddedSopInstanceUid.Value.CompareTo(sopInstanceUid.ToString()) != 0)
            {
                // the SOP instance UID doesn't match
                throw new ArgumentException("Mismatched SOP instance UID in DICOM instance");
            }

            var distinctElements = dicomElements.Select(attribute => attribute.DicomTag).Distinct();
            if (distinctElements.Count() < dicomElements.Count())
            {
                // found duplicate DICOM tags
                throw new ArgumentException("Duplicate DICOM tags");
            }

            SopInstanceUid = sopInstanceUid;
            DicomElements = dicomElements;
        }

        /// <summary>
        /// entity key for the instance
        /// </summary>
        public DicomUid EntityKey => SopInstanceUid;

        /// <summary>
        /// the SOP Instance UID
        /// </summary>
        public DicomUid SopInstanceUid 
        { 
            get; 
            private set; 
        }

        /// <summary>
        /// list of DICOM attributes for the instance
        /// </summary>
        public IEnumerable<DicomElement> DicomElements 
        { 
            get;
            private set;
        }
    }
}
