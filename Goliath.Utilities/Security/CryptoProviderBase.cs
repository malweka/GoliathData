using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Goliath.Security
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class CryptoProviderBase
    {
        /// <summary>
        /// Creates the key.
        /// </summary>
        /// <param name="asymmetricAlgorithm">The asymmetric algorithm.</param>
        /// <param name="keyFile">The key file.</param>
        /// <param name="isPrivateKey">if set to <c>true</c> [is private key].</param>
        protected void CreateKey(AsymmetricAlgorithm asymmetricAlgorithm, string keyFile, bool isPrivateKey)
        {
            using (var privateStream = new FileStream(keyFile, FileMode.Create, FileAccess.ReadWrite))
            {
                using (var streamWriter = new StreamWriter(privateStream))
                {
                    streamWriter.Write(asymmetricAlgorithm.ToXmlString(isPrivateKey));
                }
            }
        }

        /// <summary>
        /// Signs the specified private key.
        /// </summary>
        /// <param name="privateKey">The private key.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public byte[] Sign(Stream privateKey, string data)
        {
            byte[] hash;
            using (var dataStream = new MemoryStream(data.ConvertToByteArray()))
            {
                hash = Sign(privateKey, dataStream);
            }
            return hash;
        }

        /// <summary>
        /// Signs the specified private key.
        /// </summary>
        /// <param name="privateKey">The private key.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public byte[] Sign(Stream privateKey, Stream data)
        {
            byte[] hash;
            using (var reader = new StreamReader(privateKey))
            {
                hash = Sign(reader.ReadToEnd(), data);
            }

            return hash;
        }

        /// <summary>
        /// Signs the specified private key.
        /// </summary>
        /// <param name="privateKey">The private key.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public byte[] Sign(string privateKey, string data)
        {
            byte[] hash;
            using (var dataStream = new MemoryStream(data.ConvertToByteArray()))
            {
                hash = Sign(privateKey, dataStream);
            }
            return hash;
        }

        /// <summary>
        /// Signs the specified private key.
        /// </summary>
        /// <param name="privateKey">The private key.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public abstract byte[] Sign(string privateKey, Stream data);


        /// <summary>
        /// Verifies the specified public key.
        /// </summary>
        /// <param name="publicKey">The public key.</param>
        /// <param name="signature">The signature.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public bool Verify(string publicKey, string signature, Stream data)
        {
            var result = Verify(publicKey, Convert.FromBase64String(signature), data.ReadToEnd());
            return result;
        }

        /// <summary>
        /// Verifies the specified public key.
        /// </summary>
        /// <param name="publicKey">The public key.</param>
        /// <param name="signature">The signature.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public abstract bool Verify(string publicKey, byte[] signature, byte[] data);
    }
}
