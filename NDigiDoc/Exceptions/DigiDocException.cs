// Copyright (C) AS Sertifitseerimiskeskus
// This software is released under the BSD License (see LICENSE.BSD)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDigiDoc.Exceptions
{
    public class DigiDocException : Exception
    {
        public DigiDocException() : base()
        
        {
        }

        public DigiDocException(string message) : base(message)
        
        {
        }

        public DigiDocException(string message, Exception innerException) : base(message, innerException)
        
        {
        }

    }
}
