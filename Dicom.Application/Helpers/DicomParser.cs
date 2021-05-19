using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kaitai;
using DomainModel = Elekta.Capability.Dicom.Domain.Model;

namespace Elekta.Capability.Dicom.Application.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public class DicomParser : IDicomParser
    {
        public IEnumerable<DomainModel.DicomAttribute> ParseStream(Stream dicomStream)
        {
            using (var kaitaiStream = new KaitaiStream(dicomStream))            
            {
                var dicomParser = new Kaitai.Dicom(kaitaiStream);
                
                // kaitai parse puts main payload as last metadata element's Elements
                var payloadElements = dicomParser.Elements.Last().Elements;

                var selectedElements =
                    payloadElements.Select(element =>
                    {
                        var dmTag = DomainModel.DicomTag.GetTag(element.TagGroup, element.TagElem);
                        if (dmTag == null)
                            return null;

                        var value = Encoding.UTF8.GetString(element.Value);
                        return new DomainModel.DicomAttribute(dmTag, value);
                    })
                    .Where(element => element != null)
                    .ToList();

                return selectedElements;
            }
        }
    }
}
