// Copyright (C) AS Sertifitseerimiskeskus
// This software is released under the BSD License (see LICENSE.BSD)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;

using Security.Cryptography.X509Certificates;
using Security.Cryptography;

namespace NDigiDoc.Crypto
{
    public class Rsa
    {
        public static byte[] Encrypt(byte[] data, X509Certificate2 publicKey)
        {
            byte[] encData = null;
            using(var pubKey = publicKey.PublicKey.Key as RSACryptoServiceProvider)
            {
                encData = EncryptedXml.EncryptKey(data, pubKey, false);
            }

            return encData;
        }

        public static byte[] Decrypt(byte[] data, X509Certificate2 privateKey)
        {
            byte[] decrypted = null;

            if(privateKey.HasCngKey())
            {
                using (var virtualKey = privateKey.GetCngPrivateKey())
                {
                    using (var rsa = new RSACng(virtualKey))
                    {
                        rsa.EncryptionPaddingMode = AsymmetricPaddingMode.Pkcs1;
                        decrypted = rsa.DecryptValue(data);
                    }
                }
            }
            else
            {
                using (var virtualKey = privateKey.PrivateKey as RSACryptoServiceProvider)
                {
                    decrypted = virtualKey.Decrypt(data, false);
                }
            }

            return decrypted;
        }
    }
}
