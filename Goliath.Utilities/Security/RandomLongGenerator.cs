using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goliath.Security
{
    public class RandomLongGenerator : IUniqueNumberGenerator
    {
        private const int StartYear = 2000;
        private int seed;

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomLongGenerator"/> class.
        /// </summary>
        public RandomLongGenerator():this(0){}

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomLongGenerator"/> class.
        /// </summary>
        /// <param name="seed">The seed.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// seed cannot be greater than 256
        /// or
        /// seed must be greater than or equal to zero
        /// </exception>
        public RandomLongGenerator(int seed)
        {
            if (seed > 949)
                throw new ArgumentOutOfRangeException("seed cannot be greater than 256");

            if (seed < 0)
                throw new ArgumentOutOfRangeException("seed must be greater than or equal to zero");

            this.seed = seed;
        }

        int[] BuildIdParts(DateTime date)
        {
            var year = date.Year - StartYear;
            var month = date.Month;
            var day = date.Day;
            var hour = date.Hour;
            var min = date.Minute;
            var sec = date.Second;
            var ms = date.Millisecond;
            var parts = new int[] { year, month, day, hour, min, sec, ms };
            return parts;
        }

        #region IUniqueIdGenerator Members

        //public long GetNextId()
        //{
        //    return GetNextId(0);
        //}

        /// <summary>
        /// Gets the next id.
        /// </summary>
        /// <param name="seed">The seed. Seed must be lower than 950 and greater than 0</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">seed cannot be greater than 256</exception>
        public long GetNextId()
        {
            

            var date = DateTime.UtcNow;
            var idParts = BuildIdParts(date);

            var rand = new SecureRandom();
            var randVal = rand.Next(1, 50);
            var sb = new StringBuilder();

            if (seed > 0)
            {
                idParts[0] = idParts[0] + randVal + seed + 2000;
                //sb.Append(seedVal.ToString("D3"));
            }

            for (var i = 0; i < idParts.Length; i++)
            {
                if (i == 6)
                    sb.Append(idParts[i].ToString("D3"));
                else
                    sb.Append(idParts[i].ToString("D2"));
            }

            sb.Append(randVal.ToString("D2"));

            return Convert.ToInt64(sb.ToString());
        }

        #endregion
    }

    
}
