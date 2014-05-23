// Copyright (C) AS Sertifitseerimiskeskus
// This software is released under the BSD License (see LICENSE.BSD)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDigiDoc;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace NDigiDocUtility.Operations
{

    /// <summary>
    /// Example: YourExe.exe -cdoc-in tocdoc2.cdoc -cdoc-decrypt-pkcs12 testimiseks.pfx 1234 PKCS12 decrypted.txt
    /// </summary>
    internal class CDocDecrypt : AbstractOperation
    {
        internal ArgumentNode _arg;
        internal override ArgumentNode Argument { get { return _arg; } }

        internal override void InstallArgumentNodeGraph()
        {
            if (Argument != null)
            {
                return;
            }

            var recipientArg = new ArgumentNode("-cdoc-in", true, null);
            _arg = new ArgumentNode("-cdoc-decrypt-pkcs12", false, recipientArg);
        }

        internal override void Execute()
        {
            var cdocInUri = _arg.Dependencies[0].ArgumentValues[0]; // Only has 1 dependency and 1 argument value on the dependency
            var certUri = _arg.ArgumentValues[0];
            var certPassword = _arg.ArgumentValues[1];
            // var certType = _arg.ArgumentValues[2]; Discarded.
            var outputUri = _arg.ArgumentValues[3];

            var cert = new X509Certificate2(certUri, certPassword);
            var cdoc = new CDoc(new FileInfo(cdocInUri));

            var decryptedContent = cdoc.Decrypt(cert);

            using(var fs = File.Create(outputUri))
            {
                var bytes = new UTF8Encoding().GetBytes(decryptedContent);
                fs.Write(bytes, 0, bytes.Length);
            }
        }
    }
}
