// Copyright (C) AS Sertifitseerimiskeskus
// This software is released under the BSD License (see LICENSE.BSD)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NDigiDocUtility.Operations
{
    internal class ConfigureLogger : AbstractOperation
    {
        internal ArgumentNode _arg;
        internal override ArgumentNode Argument
        {
            get { return _arg; }
        }

        internal override void InstallArgumentNodeGraph()
        {
            if (Argument != null)
            {
                return;
            }

            _arg = new ArgumentNode("-log4net-xmlconfig-in", true, null);
            _arg.IsOptional = true;
        }

        internal override void Execute()
        {
            var fileUri = _arg.ArgumentValues[0];
            log4net.Config.XmlConfigurator.Configure(new FileInfo(fileUri));
        }
    }
}
