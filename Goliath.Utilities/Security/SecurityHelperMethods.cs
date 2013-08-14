using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Goliath.Security
{
    /// <summary>
    /// 
    /// </summary>
    public static class SecurityHelperMethods
    {
        /// <summary>
        /// Extracts the random salt.
        /// </summary>
        /// <param name="hashAlgorithm">The hash algorithm.</param>
        /// <param name="hashBytes">The hash bytes.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">hashBytes was not hashed with the HashAlgorithm submitted.</exception>
        public static byte[] ExtractRandomSalt(this HashAlgorithm hashAlgorithm, byte[] hashBytes)
        {
            var sizeInBytes = hashAlgorithm.HashSize / 8;

            if (hashBytes.Length < sizeInBytes)
            {
                throw new ArgumentException("hashBytes was not hashed with the HashAlgorithm submitted.");
            }

            var salt = new byte[hashBytes.Length - sizeInBytes];

            for (var i = 0; i < salt.Length; i++)
                salt[i] = hashBytes[sizeInBytes + i];

            return salt;
        }

        /// <summary>
        /// Generates the random salt.
        /// </summary>
        /// <returns></returns>
        public static byte[] GenerateRandomSalt()
        {
            var secureRandom = new SecureRandom();
            var saltArray = new byte[secureRandom.Next(8, 56)];

            using (var cryptoProv = new RNGCryptoServiceProvider())
            {
                cryptoProv.GetNonZeroBytes(saltArray);
                return saltArray;
            }
        }

        /// <summary>
        /// Merges 2 byte arrays. 
        /// </summary>
        /// <param name="array1">The array1.</param>
        /// <param name="array2">The array2.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// array1
        /// or
        /// array2
        /// </exception>
        public static byte[] MergeByteArrays(byte[] array1, byte[] array2)
        {
            //TODO: check if .Net framework doesn't already have built in functionality matching this.
            if (array1 == null || array1.Length == 0) throw new ArgumentNullException("array1");
            if (array2 == null || array2.Length == 0) throw new ArgumentNullException("array2");

            var newArray = new byte[array1.Length + array2.Length];

            for (int i = 0; i < array1.Length; i++)
                newArray[i] = array1[i];
            for (int i = 0; i < array2.Length; i++)
                newArray[array1.Length + 1] = array2[i];

            return newArray;
        }
    }
}
