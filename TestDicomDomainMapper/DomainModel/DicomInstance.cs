using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestDicomDomainMapper.DomainModel
{
    /// <summary>
    /// 
    /// </summary>
    public class DicomInstance : Seedworks.IEntity<DicomUid>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sopInstanceUid"></param>
        /// <param name="dicomAttributes"></param>
        public DicomInstance(DicomUid sopInstanceUid, IEnumerable<DicomAttribute> dicomAttributes)
        {
            SopInstanceUid = sopInstanceUid;
            DicomAttributes = dicomAttributes;
        }

        /// <summary>
        /// 
        /// </summary>
        public DicomUid EntityKey => SopInstanceUid;

        /// <summary>
        /// 
        /// </summary>
        [IgnoreMap]
        public DicomUid SopInstanceUid { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<DicomAttribute> DicomAttributes 
        { 
            get;
            private set;
        }
    }
}
