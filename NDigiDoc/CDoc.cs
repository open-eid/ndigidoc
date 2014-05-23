﻿// Copyright (C) AS Sertifitseerimiskeskus
// This software is released under the BSD License (see LICENSE.BSD)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Collections;
using System.Xml.Linq;
using System.IO;
using NDigiDoc.Exceptions;
using NDigiDoc.Crypto;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Reflection;

using Security.Cryptography.X509Certificates;
using Security.Cryptography;

namespace NDigiDoc
{
    public class CDoc
    {
        #region Constants

        public static XNamespace Denc { get { return EncryptedXml.XmlEncNamespaceUrl; } }
        public static XNamespace Ds { get { return SignedXml.XmlDsigNamespaceUrl; } }
        public static XNamespace xmlns { get { return XNamespace.Xmlns; } }

        /// <summary>
        /// Version of the library used for building the CDoc
        /// </summary>
        public static string ENC_PROP_LIB_VERSION
        {
            get { return "LibraryVersion"; }
        }
        /// <summary>
        /// Name of the encrypted documents format and version in form of 'name|version' eq. "ENCDOC-XML|1.0".
        /// </summary>
        public static string ENC_PROP_DOC_FORMAT
        {
            get { return "DocumentFormat"; }
        }
        /// <summary>
        /// The name of the document before encryption
        /// </summary>
        public static string ENC_PROP_FILE_NAME
        {
            get { return "Filename"; }
        }
        /// <summary>
        /// <para>Only used when the document to be crypted is a signed DDoc.</para>
        /// 
        /// <para>The following pattern is generated for each of the 'DataFile' xml blocks in the Signed DDoc:</para>
        /// <para>fileName|sizeInBytes|mimeType|dataFile-id-attribute</para>
        /// </summary>
        public static string ENC_PROP_ORIG_FILE
        {
            get { return "orig_file"; }
        }
        /// <summary>
        /// Size of the document in bytes before encryption
        /// </summary>
        public static string ENC_PROP_ORIG_SIZE
        {
            get { return "OriginalSize"; }
        }
        /// <summary>
        /// <para>Mime type of the document before encryption.</para>
        /// <para>For DDocs:  "http://www.sk.ee/DigiDoc/v1.3.0/digidoc.xsd"</para>
        /// <para>Others:     "http://www.isi.edu/in-noes/iana/assignments/media-types/application/zip"</para>
        /// </summary>
        public static string ENC_PROP_ORIG_MIMETYPE
        {
            get { return "OriginalMimeType"; }
        }

        public static string MIME_TYPE_ZLIB
        {
            get { return "http://www.isi.edu/in-noes/iana/assignments/media-types/application/zip"; }
        }

        #endregion

        #region Properties and attributes

        private static NDigiDoc.Logger.ILog _logger = NDigiDoc.Logger.CreateLog();

        private List<X509Certificate2> _recipients;
        /// <summary>
        /// <para>There must be at least one recipient public key specified for the encrypted CDoc.</para> 
        /// <para>A transport key is encrypted with the corresponding public key for each recipient.</para>
        /// </summary>
        public List<X509Certificate2> Recipients
        {
            get
            {
                if (_recipients == null)
                {
                    _recipients = new List<X509Certificate2>();
                }
                return _recipients;
            }
            set
            {
                _recipients = value;
            }
        }

        public bool AutoGenerateEncryptionProperties { get; set; }

        private Dictionary<string, string> _encryptionProperties;
        /// <summary>
        /// <para>Contains additional information about the CDoc. AutoGenerated unless specified otherwise.</para>
        /// </summary>
        public Dictionary<string, string> EncryptionProperties
        {
            get
            {
                if (_encryptionProperties == null)
                {
                    _encryptionProperties = new Dictionary<string, string>();
                }
                return _encryptionProperties;
            }
            set
            {
                _encryptionProperties = value;
            }
        }

        private XDocument _content;
        /// <summary>
        /// Returns a reference to the current CDocs content
        /// </summary>
        public XDocument Content { get { return _content; } }


        #endregion

        public CDoc()
        {
            AutoGenerateEncryptionProperties = true;
        }

        public CDoc(FileInfo file) : this()
        {
            Load(file);
        }

        public CDoc(XDocument cdoc) : this()
        {
            Load(cdoc);
        }

        #region Public methods

        /// <summary>
        /// Saves the CDoc on the file system
        /// </summary>
        /// <exception cref="NullReferenceException" />
        /// <param name="file"></param>
        public void Save(FileInfo file)
        {
            try
            {
                Content.Save(file.FullName);
            }
            catch(Exception up)
            {
                _logger.Error("", up);
                throw up;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="FileNotFoundException" />
        /// <param name="file"></param>
        public void Load(FileInfo file)
        {
            Load(file, null);
        }

        public void Load(XDocument cdoc)
        {
            Load(null, cdoc);
        }

        /// <summary>
        /// <para>At least one Recipient must be specified. Updated encrypted content accessable by the 'Content' property.</para>
        /// </summary>
        /// <param name="document">Data to be encrypted</param>
        public void Encrypt(string data)
        {
            Encrypt(new UTF8Encoding().GetBytes(data));
        }

        /// <summary>
        /// <para>At least one Recipient must be specified. Updated encrypted content accessable by the 'Content' property.</para>
        /// </summary>
        /// <param name="data">Data to be encrypted</param>
        /// <exception cref="DigiDocException"/>
        public void Encrypt(byte[] data)
        {
            Encrypt(data, null);
        }

        public void Encrypt(FileInfo uri)
        {
            Encrypt(null, uri.FullName);
        }


        /// <summary>
        /// Decrypts the CDoc content using the provided cert stores privateKey.
        /// </summary>
        /// <exception cref="NullReferenceException" />
        /// <exception cref="RecipientNotFoundException" />
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public string Decrypt(X509Certificate2 privateKey)
        {
            try
            {
                if (Content == null)
                {
                    throw new NullReferenceException("Property 'Content' is null, nothing to decrypt");
                }
                _logger.Debug("Decrypting CDoc with " + CDoc.ExtractRecipientName(new X509Certificate2(privateKey)) +
                              " recipients private key");

                var encDataXml = new EncryptedData();
                encDataXml.LoadXml(CDoc.XDocumentToXmlElement(Content));

                var keyEncrypted = FindRecipientsEncryptedKey(privateKey, encDataXml);
                LogFirst17Bytes("First 17 bytes or less of encrypted transport key: ", keyEncrypted);

                var transportKey = Rsa.Decrypt(keyEncrypted, new X509Certificate2(privateKey));
                LogFirst17Bytes("First 17 bytes or less of decrypted transport key: ", transportKey);

                var payload = NDigiDoc.Crypto.Aes.Decrypt(encDataXml, transportKey);
                LogFirst17Bytes("First 17 bytes or less of decrypted payload: ", payload);

                ANSIX923.RemovePadding(ref payload, true);
                if (encDataXml.MimeType != null && encDataXml.MimeType.Equals(MIME_TYPE_ZLIB))
                {
                    var payloadDecompressed = Zlib.Decompress(payload);
                    _logger.Debug("Decompressed .. Decoding finished");
                    return Encoding.UTF8.GetString(payloadDecompressed);
                }


                return Encoding.UTF8.GetString(payload);
            } 
            catch(Exception up)
            {
                _logger.Error("", up);
                throw up;
            }
        }

        #endregion

        #region NonPublic methods

        private void Load(FileInfo file, XDocument xdoc) // Climax of Load overloads
        {
            try
            {
                if(file == null)
                {
                    _content = xdoc;
                }
                else
                {
                    if (!file.Exists)
                    {
                        throw new FileNotFoundException("CDoc specified does not exist on the file system: " + file.FullName);
                    }

                    _content = XDocument.Load(file.FullName);
                }

                ExtractEncryptionProperties();
                ExtractRecipients();
            }
            catch (Exception up)
            {
                _logger.Error("", up);
                throw up;
            }
        }

        /// <summary>
        /// Climax of Encrypt overloads
        /// </summary>
        /// <exception cref="RecipientNotFoundException" />
        /// <exception cref="DigiDocException" />
        /// <param name="data">Data to be encrypted.</param>
        /// <param name="uriOrNull">File that contains the data to be encrypted. Must be null, if data is set.</param>
        protected virtual void Encrypt(byte[] data, string uriOrNull)
        {
            try
            {
                if (Recipients.Count == 0)
                {
                    throw new RecipientNotFoundException("At least one recipient must be specified before creating creating a new CDoc");
                }

                if (data != null && uriOrNull != null)
                {
                    throw new DigiDocException("Conflict: Both data and file uri for encryption specified");
                }

                byte[] plainData = ReadInputData(data, uriOrNull);
                if (AutoGenerateEncryptionProperties)
                {
                    GenerateEncryptionProperties(plainData, uriOrNull);
                }

                var compressedData = Zlib.Compress(plainData);
                LogFirst17Bytes("First 17 bytes or less of compressed data: ", compressedData);

                ANSIX923.Addpadding(ref compressedData); // legacy reasons

                byte[] sessionKey = null;
                var encData = Crypto.Aes.Encrypt(compressedData, ref sessionKey);
                LogFirst17Bytes("First 17 bytes or less of compressed data + ANSI padding with AES CBC 128 encryption: ", encData);
                LogFirst17Bytes("Generated transport key: ", sessionKey);

                // Add payload to the CDoc
                var encDataXml = new EncryptedData();

                SetEncryptionProperties(encDataXml);
                SetEncDataXmlAttributes(encDataXml);
                encDataXml.CipherData = new CipherData(encData);
                
                // Generate encrypted transport keys for recipients
                SetKeyInfo(encDataXml, sessionKey);

                _content = CDoc.XmlElementToXDocument(encDataXml.GetXml());
                CDoc.SetCDocNamespaces(_content);
                CDoc.AddLineBreaksForBase64(_content, 72);
            }
            catch(Exception up)
            {
                _logger.Error("", up);
                throw up;
            }
        }

        /// <summary>
        /// Updates the 'EncryptionProperties' property.
        /// </summary>
        /// <param name="data">Required</param>
        /// <param name="fileUriOrNull">Only used for mime type. Not required.</param>
        protected virtual void GenerateEncryptionProperties(byte[] data, string fileUriOrNull)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            
            EncryptionProperties[CDoc.ENC_PROP_LIB_VERSION] = "NDigiDoc|" + version;
            EncryptionProperties[CDoc.ENC_PROP_DOC_FORMAT] = "ENCDOC-XML|1.0";

            if (fileUriOrNull != null)
            {
                string fileName = fileUriOrNull.Split('\\', '/').Last();
                string extension = fileName.Split('.').Last();
                string mimeType = null;

                var valueReceived = MimeTypes.MimeTypesCol.TryGetValue(extension, out mimeType);
                if (!valueReceived)
                {
                    MimeTypes.MimeTypesCol.TryGetValue("txt", out mimeType);
                }

                EncryptionProperties[CDoc.ENC_PROP_FILE_NAME] = fileName;
                EncryptionProperties[CDoc.ENC_PROP_ORIG_MIMETYPE] = mimeType;
            }
            EncryptionProperties[CDoc.ENC_PROP_ORIG_SIZE] = data.Length.ToString();
        }

        private byte[] ReadInputData(byte[] data, string uriOrNull)
        {
            byte[] plainData = null;
            if (uriOrNull == null)
            {
                plainData = data;
            }
            else
            {
                using (var fs = File.OpenRead(uriOrNull))
                {
                    plainData = new byte[fs.Length];
                    fs.Read(plainData, 0, (int)fs.Length);
                }
            }

            return plainData;
        }

        /// <summary>
        /// Tries to find a matching RSA encrypted transport key for given private key. If not found, an exception is thrown.
        /// </summary>
        /// <exception cref="RecipientNotFoundException" />
        /// <param name="privateKey"></param>
        /// <param name="encDataXml"></param>
        /// <returns></returns>
        private byte[] FindRecipientsEncryptedKey(X509Certificate2 privateKey, EncryptedData encDataXml)
        {
            var en = encDataXml.KeyInfo.GetEnumerator();
            // Iterate over all EncryptedKey elements under KeyInfo node, and find the corresponding element which is defined by
            // the finding a match between the public key in xml and the private key provided for this method.
            while (en.MoveNext())
            {
                // for <denc:EncryptedKey .. clause - VALID
                if(en.Current is KeyInfoEncryptedKey)
                {
                    var currentKeyDenc = en.Current as KeyInfoEncryptedKey;
                    
                    var keyEnum = currentKeyDenc.EncryptedKey.KeyInfo.GetEnumerator(typeof(KeyInfoX509Data));
                    while (keyEnum.MoveNext())
                    {
                        var publicKey = (keyEnum.Current as KeyInfoX509Data).Certificates[0] as X509Certificate2;
                        if (ArePublicPrivateKeysMatch(publicKey, new X509Certificate2(privateKey))) // TODO: Optimize keypair validation
                        {
                            return currentKeyDenc.EncryptedKey.CipherData.CipherValue;
                        }
                    }
                }
                    // for <EncryptedKey .. clause - Invalid, go update your JDigiDoc/CDigiDoc
                else
                {
                    var currentKey = (en.Current as KeyInfoNode).Value;
                    var publicKey = new X509Certificate2(UTF8Encoding.UTF8.GetBytes(currentKey.InnerText));

                    if (ArePublicPrivateKeysMatch(publicKey, new X509Certificate2(privateKey)))
                    {
                        var keyString = currentKey.GetElementsByTagName("CipherValue", Denc.NamespaceName).Item(0).InnerText;
                        return Convert.FromBase64String(keyString);
                    }
                }

            }

            throw new RecipientNotFoundException("Recipient with name '" + CDoc.ExtractRecipientName(privateKey) + "' not found. Unable to decrypt transport key. CDoc decryption aborted.");
        }

        // TODO: REDO
        // The keypair verification should be a much simpler process than testing for encryption outputs.
        private bool ArePublicPrivateKeysMatch(X509Certificate2 publicKey, X509Certificate2 privateKey)
        {
            try
            {
                byte[] encrypted = Rsa.Encrypt(Encoding.Default.GetBytes("true"), publicKey);
                var decrypted = Encoding.Default.GetString(Rsa.Decrypt(encrypted, privateKey));
                return decrypted.Equals("true");
            }
            catch(Exception)
            {
                return false;
            }
        }

        private void ExtractRecipients()
        {
            var query = Content.Root.Descendants().Where(x => x.Name.LocalName.Equals("X509Certificate"));
            Recipients.Clear();

            foreach (var recipientCert in query)
            {
                Recipients.Add(new X509Certificate2(Convert.FromBase64String(recipientCert.Value)));
            }
        }

        /// <summary>
        /// Generates Encryption Properties based on the "Content" property
        /// </summary>
        /// <exception cref="NuullReferenceException" />
        private void ExtractEncryptionProperties()
        {
            var query = Content.Root.Descendants().Where(x => x.Name.LocalName.Equals("EncryptionProperty"));
            EncryptionProperties.Clear();

            foreach (var encProp in query)
            {
                EncryptionProperties[encProp.Attribute("Name").Value] = encProp.Value;
            }
        }

        private void SetEncDataXmlAttributes(EncryptedData encDataXml)
        {
            encDataXml.Encoding = "UTF-8";
            encDataXml.EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncAES128Url);
            encDataXml.MimeType = MIME_TYPE_ZLIB;
        }

        private void SetEncryptionProperties(EncryptedData encDataXml)
        {
            foreach (var keyVal in EncryptionProperties)
            {
                var encPropElement = new XmlDocument().CreateElement("EncryptionProperty",
                                                                     EncryptedXml.XmlEncNamespaceUrl);
                encPropElement.SetAttribute("Name", keyVal.Key);
                encPropElement.InnerText = keyVal.Value;

                var encProp = new EncryptionProperty(encPropElement);
                encDataXml.EncryptionProperties.Add(encProp);
            }
        }

        /// <summary>
        /// Generates an appropriate EncryptedKey xml block for each specified recipient
        /// </summary>
        /// <param name="encDataXml"></param>
        /// <param name="sessionKey"></param>
        private void SetKeyInfo(EncryptedData encDataXml, byte[] sessionKey)
        {
            encDataXml.KeyInfo = new KeyInfo();
            foreach (X509Certificate2 recipientOrig in Recipients)
            {
                var recipient = new X509Certificate2(recipientOrig);

                byte[] encSessionKey = Rsa.Encrypt(sessionKey, recipient);
                var encKeyXml = new EncryptedKey();
                encKeyXml.CipherData = new CipherData(encSessionKey);

                encKeyXml.EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncRSA15Url);
                encKeyXml.Recipient = CDoc.ExtractRecipientName(recipient);
                encKeyXml.KeyInfo.AddClause(new KeyInfoX509Data(recipient));

                encDataXml.KeyInfo.AddClause(new KeyInfoEncryptedKey(encKeyXml));

                LogFirst17Bytes(
                    "First 17 bytes or less for Recipients '" + CDoc.ExtractRecipientName(recipient) +
                    "' enc transport key: ", encSessionKey);
            }
        }

        #endregion

        #region Public static methods

        public static void LogFirst17Bytes(string mainMessage, byte[] bytes)
        {
            if (_logger is Logger.NoLog)
            {
                return;
            }

            var sb = new StringBuilder(15 * 3).AppendLine();
            bytes.Take(17).ToList().ForEach(x => sb.Append(x).Append(" "));
            _logger.Debug(mainMessage + " " + sb.ToString());
        }

        // Is this pretty ? No
        // Does it work ? Yes
        public static void AddLineBreaksForBase64(XDocument document, int charsUntilNewLine)
        {
            foreach(var descendant in document.Descendants().Where(x => x.Name.LocalName.Equals("CipherValue") || x.Name.LocalName.Equals("X509Certificate")))
            {
                var val = descendant.Value;
                var sb = new StringBuilder(val.Length + 100);
                for(int i = 0; i < val.Length; i += charsUntilNewLine)
                {
                    sb.Append(val.Substring(i, Math.Min(charsUntilNewLine, val.Length - i)));
                    if((val.Length - i) > charsUntilNewLine)
                    {
                        sb.Append("\n");
                    }
                }

                descendant.Value = sb.ToString();
            }

        }

        public static CDoc LoadCDoc(XDocument cdoc)
        {
            return new CDoc(cdoc);
        }

        public static CDoc LoadCDoc(FileInfo file)
        {
            var cdoc = new CDoc();
            cdoc.Load(file);
            return cdoc;
        }

        public static string ExtractRecipientName(X509Certificate2 recipient)
        {
            return recipient.GetNameInfo(X509NameType.SimpleName, false);
        }

        /// <summary>
        /// Convert the XDocument to a XmlElement object.
        /// </summary>
        /// <param name="document">The XDocument to convert into XmlElement</param>
        /// <returns></returns>
        public static XmlElement XDocumentToXmlElement(XDocument document)
        {
            XmlElement returnable = null;
            using (var reader = document.Root.CreateReader())
            {
                returnable = new XmlDocument().ReadNode(reader) as XmlElement;
            }

            return returnable;
        }

        /// <summary>
        /// Converts the XmlElement to a XDocument object.
        /// </summary>
        /// <param name="element">The XmlElement to convert into XDocument</param>
        /// <returns></returns>
        public static XDocument XmlElementToXDocument(XmlElement element)
        {
            return XDocument.Parse(element.OuterXml);
        }

        #endregion

        #region Static methods

        /// <summary>
        /// Sets the documents namespaces according to CDoc standard
        /// </summary>
        /// <param name="xdoc">The XDocument to set namespaces for</param>
        protected static void SetCDocNamespaces(XDocument xdoc)
        {
            foreach (var el in xdoc.Root.DescendantsAndSelf())
            {
                // Remove all namespace declarations from the current document
                foreach (var dec in el.Attributes().Where(x => x.IsNamespaceDeclaration))
                {
                    dec.Remove();
                }


                if (el.Name.LocalName.Equals("KeyInfo") ||
                    el.Name.LocalName.Equals("X509Data") ||
                    el.Name.LocalName.Equals("X509Certificate")) // Ns ds
                {
                    el.Name = CDoc.Ds.GetName(el.Name.LocalName);
                }
                else // Ns denc
                {
                    el.Name = CDoc.Denc.GetName(el.Name.LocalName);
                }
            }

            // Declare CDoc specified namespaces
            xdoc.Root.Add(new XAttribute(XNamespace.Xmlns + "denc", CDoc.Denc.NamespaceName));
            foreach (var keyInfo in xdoc.Descendants().Where(x => x.Name.LocalName.Equals("KeyInfo")))
            {
                keyInfo.Add(new XAttribute(XNamespace.Xmlns + "ds", CDoc.Ds.NamespaceName));
            }
        }

        #endregion
    }
}
