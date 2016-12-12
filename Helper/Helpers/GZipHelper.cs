using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

namespace PublicUtilities
{
    public class GZipHelper
    {
        public static int Compress(string sourceFile, string destinationFile)
        {
            if (!File.Exists(sourceFile))
            {
                return 0;
            }
            if (File.Exists(destinationFile))
            {
                File.Delete(destinationFile);
            }

            using (FileStream sourceFS = new FileStream(sourceFile, FileMode.Open))
            {
                byte[] buffers = new Byte[sourceFS.Length];
                ReadAllBytesFromStream(sourceFS, buffers);

                using (FileStream destFS = new FileStream(destinationFile, FileMode.Create))
                {
                    // Use the newly created memory stream for the compressed data.
                    using (GZipStream zipStream = new GZipStream(destFS, CompressionMode.Compress, true))
                    {
                        zipStream.Write(buffers, 0, buffers.Length);
                    }
                }

                return buffers.Length;
            }
        }

        private const int BUFFERSIZE = 100;
        public static void Decompress(string sourceFile, string destinationFile)
        {
            if (!File.Exists(sourceFile))
            {
                return;
            }

            try
            {
                using (FileStream fs = new FileStream(sourceFile, FileMode.Open))
                {
                    using (GZipStream zipStream = new GZipStream(fs, CompressionMode.Decompress, true))
                    {
                        //zipStream.
                        byte[] buffer = ReadAllBytesFromStream(zipStream);

                        using (FileStream destFS = new FileStream(destinationFile, FileMode.Create))
                        {
                            destFS.Write(buffer, 0, buffer.Length);
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                Trace.WriteLine("Decompress failed:" + ex.Message);
            }

        }

        private static int ReadAllBytesFromStream(Stream stream, byte[] buffer)
        {
            // Use this method is used to read all bytes from a stream.
            int offset = 0;
            int totalCount = 0;
            int buffer_len = BUFFERSIZE;
            while (stream.CanRead)
            {
                if ((buffer.Length - offset) < buffer_len)
                {
                    buffer_len = buffer.Length - offset;
                }
                int bytesRead = stream.Read(buffer, offset, buffer_len);
                if (bytesRead == 0)
                {
                    break;
                }
                offset += bytesRead;
                totalCount += bytesRead;
            }
            return totalCount;
        }

        private static byte[] ReadAllBytesFromStream(Stream stream)
        {
            // Use this method is used to read all bytes from a stream.
            int offset = 0;
            int totalCount = 0;
            List<byte> bValueList = new List<byte>();
            byte[] buffers = null;
            int buffer_len = BUFFERSIZE;

            while (stream.CanRead)
            {
                buffers = new byte[buffer_len];
                int bytesRead = stream.Read(buffers, 0, buffer_len);
                if (bytesRead == 0)
                {
                    break;
                }

                offset += bytesRead;
                totalCount += bytesRead;

                bValueList.AddRange(buffers);
            }

            buffers = new byte[bValueList.Count];
            bValueList.CopyTo(buffers, 0);
            return buffers;
        }
    }
}
