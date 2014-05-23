// Copyright (C) AS Sertifitseerimiskeskus
// This software is released under the BSD License (see LICENSE.BSD)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using NDigiDoc;
using System.IO;

namespace NDigiDocUtility.Operations
{
    /// <summary>
    /// Example: YourExe.exe -cdoc-recipient testimiseks.pem -cdoc-encrypt tocdoc.txt tocdoc1.cdoc
    /// </summary>
    internal class CDocEncrypt : AbstractOperation
    {
        internal ArgumentNode _arg;
        internal override ArgumentNode Argument { get { return _arg; } }

        internal override void InstallArgumentNodeGraph()
        {
            if(Argument != null)
            {
                return;
            }

            var recipientArg = new ArgumentNode("-cdoc-recipient", false, null);
            _arg = new ArgumentNode("-cdoc-encrypt", false, recipientArg); 
        }

        internal override void Execute()
        {
            var inputFile = _arg.ArgumentValues[0];
            var outputFile = _arg.ArgumentValues[1];
            // Only has 1 dependency, Dependency can have multiple values
            var recipientCertUris = _arg.Dependencies[0].ArgumentValues;

            var cdoc = new CDoc();
            for(int i = 0; i != recipientCertUris.Length; ++i)
            {
                X509Certificate2 cert = null;
                var uriAndPassword = recipientCertUris[i].Split('?');
                if(uriAndPassword.Length == 2)
                {
                    cert = new X509Certificate2(uriAndPassword[0], uriAndPassword[1]);
                }
                else
                {
                    cert = new X509Certificate2(recipientCertUris[i]);
                }

                cdoc.Recipients.Add(cert);
            }

            cdoc.Encrypt(new FileInfo(inputFile));

            cdoc.Save(new FileInfo(outputFile));
        }
    }
}
