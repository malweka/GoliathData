using System;

namespace Goliath.Data.Mapping.Fluent
{	
	public class PropertyMap:IMap<PropertyMap>
	{
		Property property;
		//..WithClrtType
		public PropertyMap ColumnName (string name)
		{
			return null ;
		}

		public PropertyMap Nullable (bool isNullable)
		{
			return null ;
		}

		public PropertyMap WithConstrain (string constraint)
		{
			return null ;
		}

		public PropertyMap WithSqlType (System.Data .SqlDbType dbType)
		{
			return null ;
		}

		public PropertyMap WithDbType (string dbType)
		{
			return null ;
		}
	}
   
}
  
