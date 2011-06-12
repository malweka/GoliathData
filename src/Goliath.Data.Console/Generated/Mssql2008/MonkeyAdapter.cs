using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Linq;


namespace WebZoo.Data.SqlServer
{
	public partial class MonkeyDataAdapter : DataAccessAdapter<Monkey>
	{
		const string InsertSqlConst = @"INSERT INTO monkeys(Family,CanDoTricks)
									VALUES(@Family,@CanDoTricks)";
		const string SelectSqlConst = @"SELECT Family,CanDoTricks FROM monkeys";
		
		public MonkeyDataAdapter(IDbAccess dataAccess):base(dataAccess)
		{
		}

		#region DataAccessAdapter<Monkey> implementation 

		protected override string InsertSql { get{ return InsertSqlConst; } }

		protected override string SelectSql { get{ return SelectSqlConst; } }

		protected override string UpdateSql 
		{ 
			get
			{ 
			  StringBuilder sqlBld = new StringBuilder("UPDATE monkeys SET ");
			  sqlBld.Append(string.Format("{0}=@{0}", "Family"));
			  sqlBld.Append(", ");
			  sqlBld.Append(string.Format("{0}=@{0}", "CanDoTricks"));
			  return sqlBld.ToString();
			} 
		}

		protected override DbParameter[] CreateParameters(Monkey entity)
		{
			DbParameter[] parameters = new DbParameter[]
			{ 
				dataAccess.CreateParameter(Monkey.PropertyNames.Family, entity.Family),
				dataAccess.CreateParameter(Monkey.PropertyNames.CanDoTricks, entity.CanDoTricks),
			};
			return parameters;
		}

		protected override Monkey CreateEntityFromReader(DbDataReader reader)
		{
			Monkey entity = new Monkey();
			entity.Family =  reader.ReadString("Family");
			entity.CanDoTricks =  reader.ReadBool("CanDoTricks");
			return entity;
		}

		public override int Update(Monkey entity)
		{
			return Update(entity, new QueryFilter[]{ new QueryFilter("(pk.ColumnName)", entity.(pk.PropertyName)) });
		}

		public override int UpdateBatch(IEnumerable<Monkey> list)
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

		public override Monkey SelectById(object id)
		{
			
		}  

		#endregion
	}
}

