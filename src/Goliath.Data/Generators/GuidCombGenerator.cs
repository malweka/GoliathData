﻿using System;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;

namespace Goliath.Data.Generators
{
    /// <summary>
    /// 
    /// </summary>
    public class GuidCombGenerator : IKeyGenerator<Guid>
    {
        /// <summary>
        /// 
        /// </summary>
        public const string GeneratorName = "Guid_Comb";
        #region IKeyGenerator<Guid> Members


        /// <summary>
        /// Generates the specified SQL dialect.
        /// </summary>
        /// <param name="sqlDialect">The SQL dialect.</param>
        /// <param name="entityMap">The entity map.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="priority">The priority.</param>
        /// <returns></returns>
        public Guid Generate(SqlDialect sqlDialect, EntityMap entityMap, string propertyName, out Sql.SqlOperationPriority priority)
        {
            priority = Sql.SqlOperationPriority.Low;
            return NewGuidComb();
        }

        #endregion

        #region IKeyGenerator Members

        /// <summary>
        /// Gets the type of the key.
        /// </summary>
        /// <value>
        /// The type of the key.
        /// </value>
        public Type KeyType
        {
            get { return typeof(Guid); }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get { return GeneratorName; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is database generated.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is database generated; otherwise, <c>false</c>.
        /// </value>
        public bool IsDatabaseGenerated { get { return false; } }

        /// <summary>
        /// Generates the key.
        /// </summary>
        /// <param name="sqlDialect">The SQL dialect.</param>
        /// <param name="entityMap">The entity map.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="priority">The priority.</param>
        /// <returns></returns>
        object IKeyGenerator.GenerateKey(SqlDialect sqlDialect, EntityMap entityMap, string propertyName, out Sql.SqlOperationPriority priority)
        {
            return Generate(sqlDialect, entityMap, propertyName, out priority);
        }

        #endregion

        /// <summary>
        /// Creates a new sequential guid (aka comb) see &lt;a href=&quot;http://www.informit.com/articles/article.aspx?p=25862&amp;seqNum=7&quot;&gt;http://www.informit.com/articles/article.aspx?p=25862&amp;seqNum=7&lt;/a&gt;/>.
        /// </summary>
        /// <remarks>A comb provides the benefits of a standard Guid w/o the database performance problems.</remarks>
        /// <returns>A new sequential guid (comb).</returns>
        public static Guid NewGuidComb()
        {
            //Guid comb algorithm below Copyright Mark J. Miller
            //http://www.developmentalmadness.com/archive/2010/09/28/sequential-guid-algorithm-ndash-implementing-combs-in-.net.aspx

            byte[] uid = Guid.NewGuid().ToByteArray();
            byte[] binDate = BitConverter.GetBytes(DateTime.UtcNow.Ticks); // use UTC now to prevent conflicts w/ date time savings

            // create comb in SQL Server sort order
            byte[] comb = new byte[uid.Length];

            // the first 7 bytes are random - if two combs
            // are generated at the same point in time
            // they are not guaranteed to be sequential.
            // But for every DateTime.Tick there are
            // 72,057,594,037,927,935 unique possibilities so
            // there shouldn't be any collisions
            comb[3] = uid[0];
            comb[2] = uid[1];
            comb[1] = uid[2];
            comb[0] = uid[3];
            comb[5] = uid[4];
            comb[4] = uid[5];
            comb[7] = uid[6];

            // set the first 'nibble of the 7th byte to '1100' so 
            // later we can validate it was generated by us
            comb[6] = (byte)(0xc0 | (0xf & uid[7]));

            // the last 8 bytes are sequential,
            // these will reduce index fragmentation
            // to a degree as long as there are not a large
            // number of Combs generated per millisecond
            comb[9] = binDate[0];
            comb[8] = binDate[1];
            comb[15] = binDate[2];
            comb[14] = binDate[3];
            comb[13] = binDate[4];
            comb[12] = binDate[5];
            comb[11] = binDate[6];
            comb[10] = binDate[7];

            return new Guid(comb);
        }
    }
}
