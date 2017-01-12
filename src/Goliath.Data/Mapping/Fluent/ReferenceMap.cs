using System;

namespace Goliath.Data.Mapping.Fluent
{
    /// <summary>
    /// 
    /// </summary>
    public class ReferenceMap : IMap<ReferenceMap>
    {
        
        /// <summary>
        /// Withes the type of the relatioship.
        /// </summary>
        /// <param name="relationshipType">Type of the relationship.</param>
        /// <returns></returns>
        public ReferenceMap WithRelatioshipType(RelationshipType relationshipType)
        {
            return null;
        }

        //public ReferenceMap WithReferencedEntity(string referencedEntity)
        //{
        //    return null;
        //}

        /// <summary>
        /// Withes the reference constraint.
        /// </summary>
        /// <param name="constraint">The constraint.</param>
        /// <returns></returns>
        public ReferenceMap WithReferenceConstraint(string constraint)
        {
            return null;
        }

        /// <summary>
        /// Columns the name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public ReferenceMap ColumnName(string name)
        {
            return null;
        }

        /// <summary>
        /// Nullables the specified is nullable.
        /// </summary>
        /// <param name="isNullable">if set to <c>true</c> [is nullable].</param>
        /// <returns></returns>
        public ReferenceMap Nullable(bool isNullable)
        {
            return null;
        }

        /// <summary>
        /// Withes the constrain.
        /// </summary>
        /// <param name="constraint">The constraint.</param>
        /// <returns></returns>
        public ReferenceMap WithConstrain(string constraint)
        {
            return null;
        }

        /// <summary>
        /// Withes the type of the SQL.
        /// </summary>
        /// <param name="dbType">Type of the db.</param>
        /// <returns></returns>
        public ReferenceMap WithSqlType(System.Data.SqlDbType dbType)
        {
            return null;
        }

        /// <summary>
        /// Withes the type of the db.
        /// </summary>
        /// <param name="dbType">Type of the db.</param>
        /// <returns></returns>
        public ReferenceMap WithDbType(string dbType)
        {
            return null;
        }
    }

}

