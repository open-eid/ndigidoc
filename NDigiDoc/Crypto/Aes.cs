﻿// Copyright (C) AS Sertifitseerimiskeskus
// This software is released under the BSD License (see LICENSE.BSD)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography;
using NDigiDoc.Exceptions;

namespace NDigiDoc.Crypto
{
    public class Aes
    {
        private static NDigiDoc.Logger.ILog _logger = NDigiDoc.Logger.CreateLog();

        /// <summary>
        /// Encrypts data with AES CBC 128
        /// </summary>
        /// <param name="data">Data to be encrypted</param>
        /// <param name="transportKey">Transport\Session key generated during encryption</param>
        /// <returns>Data encrypted with AES CBC 128</returns>
        public static byte[] Encrypt(byte[] data, ref byte[] transportKey)
        {
            return Encrypt(data, null, ref transportKey);
        }

        /// <summary>
        /// Encrypts data with AES CBC 128
        /// </summary>
        /// <param name="data">Data to be encrypted</param>
        /// <param name="key">Transport\Session key to use during encryption</param>
        /// <returns>Data encrypted with AES CBC 128</returns>
        public static byte[] Encrypt(byte[] data, byte[] key)
        {
            byte[] dummy = null;
            return Encrypt(data, key, ref dummy);
        }

        /// <summary>
        /// Encrypts data with AES CBC 128, IV zeros. Climax of encrypt overloads.
        /// </summary>
        /// <param name="data">Data to be encrypted</param>
        /// <param name="key">Aes transport key or null for autogeneration</param>
        /// <param name="autoGeneratedKey">If param. 'key' is null, a valid key will be stored in this parameter</param>
        /// <returns>Data encrypted with AES CBC 128</returns>
        private static byte[] Encrypt(byte[] data, byte[] key, ref byte[] autoGeneratedKey)
        {
            byte[] encryptedBytes = null;
            var encryptedXml = new EncryptedXml();
            using (var aes = new AesManaged())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.KeySize = 128;
                aes.BlockSize = 128;
                aes.IV = new byte[16];
                if(key == null)
                {
                    aes.GenerateKey();    
                }
                else
                {
                    aes.Key = key;
                }

                autoGeneratedKey = aes.Key;
                encryptedXml.Mode = CipherMode.CBC;
                encryptedXml.Padding = PaddingMode.PKCS7;
                encryptedBytes = encryptedXml.EncryptData(data, aes);
            }

            return encryptedBytes;
        }

        public static byte[] Decrypt(EncryptedData encDataXml, byte[] transportKey)
        {
            byte[] decrypted = null;
            using (var aes = new AesManaged())
            {
                aes.Key = transportKey;
                aes.IV = new byte[16];
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                var encXml = new EncryptedXml();
                encXml.Padding = PaddingMode.PKCS7; // TODO: Undo
                decrypted = encXml.DecryptData(encDataXml, aes);
            }

            return decrypted;
        }
    }
}
