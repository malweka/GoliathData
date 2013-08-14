using System;
using System.Security.Cryptography;

namespace Goliath.Security
{
    /// <summary>
    /// 
    /// </summary>
    public class SaltedSha256HashProvider : IHashProvider
    {
        /// <summary>
        /// The provider name
        /// </summary>
        public const string ProviderName = "SSHA256";

        #region IHashProvider Members

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get { return ProviderName; }
        }

        /// <summary>
        /// Computes the hash.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">data</exception>
        public byte[] ComputeHash(byte[] data)
        {
            if (data == null || data.Length == 0) throw new ArgumentNullException("data");
            var saltArray = SecurityHelperMethods.GenerateRandomSalt();
            return ComputeHash(data, saltArray);
        }

        /// <summary>
        /// Computes the hash.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="saltArray">The salt array.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">data</exception>
        public byte[] ComputeHash(byte[] data, byte[] saltArray)
        {
            if (data == null || data.Length == 0) throw new ArgumentNullException("data");

            using (var managedHashProvider = new SHA256Managed())
            {
                var saltedData = SecurityHelperMethods.MergeByteArrays(data, saltArray);

                var hash = managedHashProvider.ComputeHash(saltedData);
                var saltedHash = SecurityHelperMethods.MergeByteArrays(hash, saltArray);

                return saltedHash;
            }
        }

        /// <summary>
        /// Verifies the hash.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="hash">The hash.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool VerifyHash(byte[] data, byte[] hash)
        {
            if (data == null || data.Length == 0) throw new ArgumentNullException("data");
            if (hash == null || hash.Length == 0) throw new ArgumentNullException("hash");

            using (var managedHashProvider = new SHA256Managed())
            {
                var salt = managedHashProvider.ExtractRandomSalt(hash);
                var computedHash = ComputeHash(data, salt);
                return string.Equals(Convert.ToBase64String(hash), Convert.ToBase64String(computedHash));
            }
        }

        #endregion






    }
}