// Copyright (C) AS Sertifitseerimiskeskus
// This software is released under the BSD License (see LICENSE.BSD)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDigiDoc.Exceptions
{
    public class RecipientNotFoundException : DigiDocException
    {
        public RecipientNotFoundException() : base()
        
        {
        }

        public RecipientNotFoundException(string message) : base(message)
        
        {
        }

        public RecipientNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        
        {
        }
    }
}
