using System;

namespace Goliath.Data.Mapping.Fluent
{
	public class ReferenceMap:IMap<ReferenceMap>
	{
		Relation relation;
		
		public ReferenceMap WithRelatioshipType (RelationshipType relationshipType)
		{
			return null ;
		}

		//public ReferenceMap WithReferencedEntity(string referencedEntity)
		//{
		//    return null;
		//}

		public ReferenceMap WithReferenceConstraint (string constraint)
		{
			return null ;
		}
		
		public ReferenceMap ColumnName (string name)
		{
			return null ;
		}

		public ReferenceMap Nullable (bool isNullable)
		{
			return null ;
		}

		public ReferenceMap WithConstrain (string constraint)
		{
			return null ;
		}

		public ReferenceMap WithSqlType (System.Data.SqlDbType dbType)
		{
			return null ;
		}

		public ReferenceMap WithDbType (string dbType)
		{
			return null ;
		}
	}

}

