using System;

namespace Goliath.Data.Mapping.Fluent
{
	public class PrimaryKeyMap:IMap<PrimaryKeyMap>
	{
		PrimaryKey primaryKey;
		
		public PrimaryKeyMap ColumnName (string name)
		{
			return null ;
		}

		public PrimaryKeyMap Nullable (bool isNullable)
		{
			return null ;
		}

		public PrimaryKeyMap WithConstrain (string constraint)
		{
			return null ;
		}

		public PrimaryKeyMap WithSqlType (System.Data .SqlDbType dbType)
		{
			return null ;
		}

		public PrimaryKeyMap WithDbType (string dbType)
		{
			return null ;
		}
		
		public PrimaryKeyMap Identity (bool isIdentity)
		{
			return null ;
		}

		public PrimaryKeyMap AutoGeenration (bool isAuto)
		{
			return null ;
		}

		public PrimaryKeyMap WithKeyGenerator (string generator)
		{
			return null ;
		}
	}
}

