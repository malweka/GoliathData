using System;

namespace Goliath.Data.Mapping.Fluent
{
    /// <summary>
    /// 
    /// </summary>
    public class PrimaryKeyMap : IMap<PrimaryKeyMap>
    {
         

        /// <summary>
        /// Columns the name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public PrimaryKeyMap ColumnName(string name)
        {
            return null;
        }

        /// <summary>
        /// Nullables the specified is nullable.
        /// </summary>
        /// <param name="isNullable">if set to <c>true</c> [is nullable].</param>
        /// <returns></returns>
        public PrimaryKeyMap Nullable(bool isNullable)
        {
            return null;
        }

        /// <summary>
        /// Withes the constrain.
        /// </summary>
        /// <param name="constraint">The constraint.</param>
        /// <returns></returns>
        public PrimaryKeyMap WithConstrain(string constraint)
        {
            return null;
        }

        /// <summary>
        /// Withes the type of the SQL.
        /// </summary>
        /// <param name="dbType">Type of the db.</param>
        /// <returns></returns>
        public PrimaryKeyMap WithSqlType(System.Data.SqlDbType dbType)
        {
            return null;
        }

        /// <summary>
        /// Withes the type of the db.
        /// </summary>
        /// <param name="dbType">Type of the db.</param>
        /// <returns></returns>
        public PrimaryKeyMap WithDbType(string dbType)
        {
            return null;
        }

        /// <summary>
        /// Identities the specified is identity.
        /// </summary>
        /// <param name="isIdentity">if set to <c>true</c> [is identity].</param>
        /// <returns></returns>
        public PrimaryKeyMap Identity(bool isIdentity)
        {
            return null;
        }

        /// <summary>
        /// auto generates the key.
        /// </summary>
        /// <param name="isAuto">if set to <c>true</c> [is auto].</param>
        /// <returns></returns>
        public PrimaryKeyMap AutoGeneration(bool isAuto)
        {
            return null;
        }

        /// <summary>
        /// Withes the key generator.
        /// </summary>
        /// <param name="generator">The generator.</param>
        /// <returns></returns>
        public PrimaryKeyMap WithKeyGenerator(string generator)
        {
            return null;
        }
    }
}

