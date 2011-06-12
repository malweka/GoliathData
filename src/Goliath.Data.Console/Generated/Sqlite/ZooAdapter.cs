using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Linq;


namespace WebZoo.Data.Sqlite
{
	public partial class ZooDataAdapter : DataAccessAdapter<Zoo>
	{
		const string InsertSqlConst = @"INSERT INTO zoos(Name,City,AcceptNewAnimals)
									VALUES(@Name,@City,@AcceptNewAnimals)";
		const string SelectSqlConst = @"SELECT Name,City,AcceptNewAnimals FROM zoos";
		
		public ZooDataAdapter(IDbAccess dataAccess):base(dataAccess)
		{
		}

		#region DataAccessAdapter<Zoo> implementation 

		protected override string InsertSql { get{ return InsertSqlConst; } }

		protected override string SelectSql { get{ return SelectSqlConst; } }

		protected override string UpdateSql 
		{ 
			get
			{ 
			  StringBuilder sqlBld = new StringBuilder("UPDATE zoos SET ");
			  sqlBld.Append(string.Format("{0}=@{0}", "Name"));
			  sqlBld.Append(", ");
			  sqlBld.Append(string.Format("{0}=@{0}", "City"));
			  sqlBld.Append(", ");
			  sqlBld.Append(string.Format("{0}=@{0}", "AcceptNewAnimals"));
			  return sqlBld.ToString();
			} 
		}

		protected override DbParameter[] CreateParameters(Zoo entity)
		{
			DbParameter[] parameters = new DbParameter[]
			{ 
				dataAccess.CreateParameter(Zoo.PropertyNames.Name, entity.Name),
				dataAccess.CreateParameter(Zoo.PropertyNames.City, entity.City),
				dataAccess.CreateParameter(Zoo.PropertyNames.AcceptNewAnimals, entity.AcceptNewAnimals),
			};
			return parameters;
		}

		protected override Zoo CreateEntityFromReader(DbDataReader reader)
		{
			Zoo entity = new Zoo();
			entity.Name =  reader.ReadString("Name");
			entity.City =  reader.ReadString("City");
			entity.AcceptNewAnimals =  reader.ReadBool("AcceptNewAnimals");
			return entity;
		}

		public override int Update(Zoo entity)
		{
			return Update(entity, new QueryFilter[]{ new QueryFilter("(pk.ColumnName)", entity.(pk.PropertyName)) });
		}

		public override int UpdateBatch(IEnumerable<Zoo> list)
		{
			 try
			 {
				int executed = 0;
				using (DbConnection connection = dataAccess.CreateNewConnection())
				{
				   lock (dataAccess.LockObject)
				   {
					  using (DbTransaction dbTrans = connection.BeginTransaction())
					  {
						 foreach (var entity in list)
						 {						 
							executed += ExecuteUpdate(connection, entity, new QueryFilter[]{ new QueryFilter("(pk.ColumnName)", entity.(pk.PropertyName)) });
						 }
						 dbTrans.Commit();
					  }
				   }
				}
				return executed;
			 }
			 catch (Exception ex)
			 {
				throw new DataAccessException("Couldn't save data into data store", ex);
			 }
		}

		public override Zoo SelectById(object id)
		{
			
		}  

		#endregion
	}
}

