// Copyright (C) AS Sertifitseerimiskeskus
// This software is released under the BSD License (see LICENSE.BSD)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDigiDoc.Crypto
{
    public static class ANSIX923
    {
        private static readonly int BLOCK_SIZE_BYTES = 16;

        public static void Addpadding(ref byte[] data)
        {
            int mod = data.Length % BLOCK_SIZE_BYTES;
            int paddingLength = BLOCK_SIZE_BYTES - mod;
            byte[] padding;

            padding = new byte[paddingLength];
            padding[paddingLength - 1] = (byte)paddingLength;

            int dataOriginalLength = data.Length;
            Array.Resize<byte>(ref data, dataOriginalLength + paddingLength);
            Array.Copy(padding, 0, data, dataOriginalLength, padding.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="checkForNoPadding">If true, a check on the existance of padding is executed.</param>
        public static void RemovePadding(ref byte[] data, bool checkForNoPadding)
        {
            var lastByte = (int)data[data.Length - 1];
            if(checkForNoPadding)
            {
                int zeroCount = lastByte - 1;
                var zeros = new byte[zeroCount];

                Array.Copy(data, data.Length - lastByte, zeros, 0, zeroCount);
                for (int i = 0; i != zeroCount; ++i)
                {
                    if (zeros[i] != 0)
                    {
                        return;
                    }
                }
            }

            Array.Resize<byte>(ref data, data.Length - lastByte);
        }
    }
}
