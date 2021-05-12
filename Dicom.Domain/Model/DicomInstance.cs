using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Dicom.Domain.Seedwork;

namespace Dicom.Domain.Model
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
        /// <param name="dicomAttributes"></param>
        public DicomInstance(DicomUid sopInstanceUid, IEnumerable<DicomAttribute> dicomAttributes)
        {
            var embeddedSopInstanceUid = 
                dicomAttributes.SingleOrDefault(attribute => attribute.DicomTag.Equals(DicomTag.SOPINSTANCEUID));
            if (embeddedSopInstanceUid == null)
            {
                // the SOP instance UID is missing
                throw new ArgumentException();
            }
            else if (embeddedSopInstanceUid.Value.CompareTo(sopInstanceUid.ToString()) != 0)
            {
                // the SOP instance UID doesn't match
                throw new ArgumentException();
            }

            SopInstanceUid = sopInstanceUid;
            DicomAttributes = dicomAttributes;
        }

        /// <summary>
        /// entity key for the instance
        /// </summary>
        public DicomUid EntityKey => SopInstanceUid;

        /// <summary>
        /// the SOP Instance UID
        /// </summary>
        [IgnoreMap]
        public DicomUid SopInstanceUid 
        { 
            get; 
            private set; 
        }

        /// <summary>
        /// list of DICOM attributes for the instance
        /// </summary>
        public IEnumerable<DicomAttribute> DicomAttributes 
        { 
            get;
            private set;
        }
    }
}
