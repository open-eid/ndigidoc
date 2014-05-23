// Copyright (C) AS Sertifitseerimiskeskus
// This software is released under the BSD License (see LICENSE.BSD)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDigiDoc.Crypto
{
    public class Zlib
    {
        /// <summary>
        /// Compresses data using the ZLib stream
        /// </summary>
        /// <param name="data">Data to compress</param>
        /// <returns></returns>
        public static byte[] Compress(byte[] data)
        {
            return Ionic.Zlib.ZlibStream.CompressBuffer(data);
        }

        /// <summary>
        /// Decompresses data using the ZLib stream. Handles ANSI X.923 padding, if neccessary.
        /// </summary>
        /// <param name="data">Data to decompress</param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] data)
        {
            return Ionic.Zlib.ZlibStream.UncompressBuffer(data);
        }
    }
}
