using System;
using System.IO;
using System.IO.Compression;

namespace SharedComponents.Utility
{
    public static class Zlib
    {
        public static byte[] Decompress(byte[] data)
        {
            byte[] decompressedArray = null;
            try
            {
                using (var decompressedStream = new MemoryStream())
                {
                    using (var compressStream = new MemoryStream(data, 2, data.Length - 2)) // zlib header removal
                    {
                        using (var deflateStream = new DeflateStream(compressStream, CompressionMode.Decompress))
                        {
                            deflateStream.CopyTo(decompressedStream);
                        }
                    }
                    decompressedArray = decompressedStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return decompressedArray;
        }
    }
}