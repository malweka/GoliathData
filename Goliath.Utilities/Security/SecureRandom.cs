using System;
using System.Security.Cryptography;

namespace Goliath.Security
{
    //NOTE: code from http://www.informit.com/guides/content.aspx?g=dotnet&seqNum=775 used.
    /// <summary>
    /// 
    /// </summary>
    public class SecureRandom : Random
    {
        public const int BufferSize = 1024;
        readonly byte[] randomBuff;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SecureRandom"/> class.
        /// </summary>
        public SecureRandom()
        {
            randomBuff = new byte[BufferSize];
        }

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to zero and less than <see cref="F:System.Int32.MaxValue" />.
        /// </returns>
        public override int Next()
        {
            using (var rngProvider = RandomNumberGenerator.Create())
            {
                rngProvider.GetBytes(randomBuff);
            }

            var nxt = BitConverter.ToInt32(randomBuff, 0) & 0x7fffffff;
            return nxt;
        }

        /// <summary>
        /// Returns a nonnegative random number less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated. <paramref name="maxValue" /> must be greater than or equal to zero.</param>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to zero, and less than <paramref name="maxValue" />; that is, the range of return values ordinarily includes zero but not <paramref name="maxValue" />. However, if <paramref name="maxValue" /> equals zero, <paramref name="maxValue" /> is returned.
        /// </returns>
        public override int Next(int maxValue)
        {
            return Next() % maxValue;
        }

        /// <summary>
        /// Nexts the specified min.
        /// </summary>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">min MUST be greater than </exception>
        public override int Next(int min, int max)
        {
            if (min > max)
                throw new ArgumentOutOfRangeException("min MUST be greater than ");

            var range = max - min;
            return min + Next(range);
        }

        /// <summary>
        /// Returns a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>
        /// A double-precision floating point number greater than or equal to 0.0, and less than 1.0.
        /// </returns>
        public override double NextDouble()
        {
            var val = Next();
            return (double)val / int.MaxValue;
        }

        /// <summary>
        /// Fills the elements of a specified array of bytes with random numbers.
        /// </summary>
        /// <param name="buffer">An array of bytes to contain random numbers.</param>
        public override void NextBytes(byte[] buffer)
        {
            using (var rngProvider = RandomNumberGenerator.Create())
            {
                //offset = randomBuff.Length;
                rngProvider.GetBytes(randomBuff);
            }
        }
    }
}