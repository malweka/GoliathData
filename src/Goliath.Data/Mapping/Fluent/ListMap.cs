using System;

namespace Goliath.Data.Mapping.Fluent
{
	public class ListMap:IMap<ListMap>
	{
		public ListMap ColumnName (string name)
		{
			return null ;
		}

		public ListMap Nullable (bool isNullable)
		{
			return null ;
		}

		public ListMap WithConstrain (string constraint)
		{
			return null ;
		}

		public ListMap WithSqlType (System.Data.SqlDbType dbType)
		{
			return null ;
		}

		public ListMap WithDbType (string dbType)
		{
			return null ;
		}
		
		public ListMap WithRelatioshipType (RelationshipType relationshipType)
		{
			return null ;
		}

		public ListMap WithMapTable (string mapTable)
		{
			return null ;
		}

		public ListMap WithMapReferenceColumn (string mapTable)
		{
			return null ;
		}

		public ListMap WithMapColumn (string mapTable)
		{
			return null ;
		}

		public ListMap IsInverse (bool isInverse)
		{
			return null ;
		}

		public ListMap WithReferencedProperty (string referencedProperty)
		{
			return null ;
		}
	}

}

