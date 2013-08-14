using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Goliath
{
    /// <summary>
    /// 
    /// </summary>
    public static class IOHelpers
    {
        /// <summary>
        /// Convert the string to a byte array
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static byte[] ConvertToByteArray(this string text)
        {
            var bytes = new byte[text.Length * sizeof(char)];
            Buffer.BlockCopy(text.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static string ConvertToString(this byte[] bytes)
        {
            var chars = new char[bytes.Length / sizeof(char)];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        /// <summary>
        /// Converts bytes to string
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
        public static string ConvertToBase64String(this byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Converts the byte array to hex string.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
        public static string ConvertToHexString(this byte[] bytes)
        {
            var hexStringBuilder = new StringBuilder(64);

            for (int i = 0; i < bytes.Length; i++)
            {
                hexStringBuilder.Append(String.Format("{0:X2}", bytes[i]));
            }
            return hexStringBuilder.ToString();
        }

        /// <summary>
        /// Reads stream to byte array.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public static byte[] ReadToEnd(this Stream stream)
        {
            long originalPosition = stream.Position;
            try
            {
                stream.Position = 0;
                byte[] readByteBuffer = new byte[4096];
                using (MemoryStream memStream = new MemoryStream())
                {
                    while (true)
                    {
                        int read = stream.Read(readByteBuffer, 0, readByteBuffer.Length);

                        if (read <= 0)
                            return memStream.ToArray();

                        memStream.Write(readByteBuffer, 0, read);
                    }
                }
            }
            finally
            {
                stream.Position = originalPosition;
            }
        }

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="content">The content.</param>
        public static void CopyTo(this Stream stream, byte[] content)
        {
            try
            {
                using (var bw = new BinaryWriter(stream))
                {
                    bw.Write(content);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        const int bufferSize = 2048;

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="inStream">The in stream.</param>
        /// <param name="outStream">The out stream.</param>
        public static void CopyTo(this Stream inStream, Stream outStream)
        {
            byte[] buffer = new byte[bufferSize];
            int bytes = 0;
            while ((bytes = inStream.Read(buffer, 0, bufferSize)) > 0)
            {
                outStream.Write(buffer, 0, bytes);
            }
        }
    }
}
