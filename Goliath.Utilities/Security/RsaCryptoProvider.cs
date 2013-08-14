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
    public class RsaCryptoProvider : CryptoProviderBase, ICryptoProvider
    {
        #region ICryptoProvider Members

        /// <summary>
        /// Generates the key pair.
        /// </summary>
        /// <param name="keySize">Size of the key.</param>
        /// <param name="keyFileName">Name of the key file.</param>
        /// <param name="keyStoreLocation">The key store location.</param>
        public void GenerateKeyPair(int keySize, string keyFileName, string keyStoreLocation)
        {
            using (var provider = new RSACryptoServiceProvider(keySize) { PersistKeyInCsp = false })
            {
                CreateKey(provider, Path.Combine(keyStoreLocation, keyFileName + "_private.key"), true);
                CreateKey(provider, Path.Combine(keyStoreLocation, keyFileName + "_public.key"), false);
            }
        }

        /// <summary>
        /// Signs the specified private key.
        /// </summary>
        /// <param name="privateKey">The private key.</param>
        /// <param name="data">The armoredData.</param>
        /// <returns></returns>
        public override byte[] Sign(string privateKey, Stream data)
        {
            byte[] hash;
            using (var provider = new RSACryptoServiceProvider())
            {
                provider.FromXmlString(privateKey);
                hash = provider.SignData(data, CryptoConfig.MapNameToOID("SHA512"));
            }

            return hash;
        }

        /// <summary>
        /// Verifies the specified public key.
        /// </summary>
        /// <param name="publicKey">The public key.</param>
        /// <param name="signature">The signature.</param>
        /// <param name="data">The armoredData.</param>
        /// <returns></returns>
        public override bool Verify(string publicKey, byte[] signature, byte[] data)
        {
            bool result;
            using (var provider = new RSACryptoServiceProvider())
            {
                provider.FromXmlString(publicKey);
                result = provider.VerifyData(data, CryptoConfig.MapNameToOID("SHA512"), signature);
            }

            return result;
        }

        #endregion

        /// <summary>
        /// Encrypts the specified public key.
        /// </summary>
        /// <param name="publicKey">The public key.</param>
        /// <param name="data">The armoredData.</param>
        /// <returns></returns>
        public byte[] Encrypt(string publicKey, byte[] data)
        {
            using (var provider = new RSACryptoServiceProvider())
            {
                provider.FromXmlString(publicKey);
                return provider.Encrypt(data, true);
            }
        }

        /// <summary>
        /// Decrypts the specified private key.
        /// </summary>
        /// <param name="privateKey">The private key.</param>
        /// <param name="armoredData">The armoredData.</param>
        /// <returns></returns>
        public byte[] Decrypt(string privateKey, byte[] armoredData)
        {
            using (var provider = new RSACryptoServiceProvider())
            {
                provider.FromXmlString(privateKey);
                return provider.Decrypt(armoredData, true);
            }
        }
    }
}
