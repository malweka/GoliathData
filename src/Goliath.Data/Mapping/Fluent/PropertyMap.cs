using System;

namespace Goliath.Data.Mapping.Fluent
{
    /// <summary>
    /// 
    /// </summary>
    public class PropertyMap : IMap<PropertyMap>
    {
        /// <summary>
        /// Columns the name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public PropertyMap ColumnName(string name)
        {
            return null;
        }

        /// <summary>
        /// Nullables the specified is nullable.
        /// </summary>
        /// <param name="isNullable">if set to <c>true</c> [is nullable].</param>
        /// <returns></returns>
        public PropertyMap Nullable(bool isNullable)
        {
            return null;
        }

        /// <summary>
        /// Withes the constrain.
        /// </summary>
        /// <param name="constraint">The constraint.</param>
        /// <returns></returns>
        public PropertyMap WithConstrain(string constraint)
        {
            return null;
        }

        /// <summary>
        /// Withes the type of the SQL.
        /// </summary>
        /// <param name="dbType">Type of the db.</param>
        /// <returns></returns>
        public PropertyMap WithSqlType(System.Data.SqlDbType dbType)
        {
            return null;
        }

        /// <summary>
        /// Withes the type of the db.
        /// </summary>
        /// <param name="dbType">Type of the db.</param>
        /// <returns></returns>
        public PropertyMap WithDbType(string dbType)
        {
            return null;
        }
    }

}

