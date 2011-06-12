using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Linq;


namespace WebZoo.Data.SqlServer
{
	public partial class AnimalDataAdapter : DataAccessAdapter<Animal>
	{
		const string InsertSqlConst = @"INSERT INTO animals(Name,Age,Location,ReceivedOn)
									VALUES(@Name,@Age,@Location,@ReceivedOn)";
		const string SelectSqlConst = @"SELECT Name,Age,Location,ReceivedOn FROM animals";
		
		public AnimalDataAdapter(IDbAccess dataAccess):base(dataAccess)
		{
		}

		#region DataAccessAdapter<Animal> implementation 

		protected override string InsertSql { get{ return InsertSqlConst; } }

		protected override string SelectSql { get{ return SelectSqlConst; } }

		protected override string UpdateSql 
		{ 
			get
			{ 
			  StringBuilder sqlBld = new StringBuilder("UPDATE animals SET ");
			  sqlBld.Append(string.Format("{0}=@{0}", "Name"));
			  sqlBld.Append(", ");
			  sqlBld.Append(string.Format("{0}=@{0}", "Age"));
			  sqlBld.Append(", ");
			  sqlBld.Append(string.Format("{0}=@{0}", "Location"));
			  sqlBld.Append(", ");
			  sqlBld.Append(string.Format("{0}=@{0}", "ReceivedOn"));
			  return sqlBld.ToString();
			} 
		}

		protected override DbParameter[] CreateParameters(Animal entity)
		{
			DbParameter[] parameters = new DbParameter[]
			{ 
				dataAccess.CreateParameter(Animal.PropertyNames.Name, entity.Name),
				dataAccess.CreateParameter(Animal.PropertyNames.Age, entity.Age),
				dataAccess.CreateParameter(Animal.PropertyNames.Location, entity.Location),
				dataAccess.CreateParameter(Animal.PropertyNames.ReceivedOn, entity.ReceivedOn),
			};
			return parameters;
		}

		protected override Animal CreateEntityFromReader(DbDataReader reader)
		{
			Animal entity = new Animal();
			entity.Name =  reader.ReadString("Name");
			entity.Age =  reader.ReadFloat("Age");
			entity.Location =  reader.ReadString("Location");
			entity.ReceivedOn =  reader.ReadDateTime("ReceivedOn");
			return entity;
		}

		public override int Update(Animal entity)
		{
			return Update(entity, new QueryFilter[]{ new QueryFilter("(pk.ColumnName)", entity.(pk.PropertyName)) });
		}

		public override int UpdateBatch(IEnumerable<Animal> list)
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

		public override Animal SelectById(object id)
		{
			
		}  

		#endregion
	}
}

