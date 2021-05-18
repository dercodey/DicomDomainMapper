﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dicom.Api.Abstractions
{
    public class DicomElement
    {
        public string DicomTag { get; set; }

        public string Value { get; set; }
    }
}