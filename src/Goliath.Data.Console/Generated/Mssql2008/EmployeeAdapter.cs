using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Linq;


namespace WebZoo.Data.SqlServer
{
	public partial class EmployeeDataAdapter : DataAccessAdapter<Employee>
	{
		const string InsertSqlConst = @"INSERT INTO employees(FirstName,LastName,EmailAddress,Telephone,Title,HiredOn)
									VALUES(@FirstName,@LastName,@EmailAddress,@Telephone,@Title,@HiredOn)";
		const string SelectSqlConst = @"SELECT FirstName,LastName,EmailAddress,Telephone,Title,HiredOn FROM employees";
		
		public EmployeeDataAdapter(IDbAccess dataAccess):base(dataAccess)
		{
		}

		#region DataAccessAdapter<Employee> implementation 

		protected override string InsertSql { get{ return InsertSqlConst; } }

		protected override string SelectSql { get{ return SelectSqlConst; } }

		protected override string UpdateSql 
		{ 
			get
			{ 
			  StringBuilder sqlBld = new StringBuilder("UPDATE employees SET ");
			  sqlBld.Append(string.Format("{0}=@{0}", "FirstName"));
			  sqlBld.Append(", ");
			  sqlBld.Append(string.Format("{0}=@{0}", "LastName"));
			  sqlBld.Append(", ");
			  sqlBld.Append(string.Format("{0}=@{0}", "EmailAddress"));
			  sqlBld.Append(", ");
			  sqlBld.Append(string.Format("{0}=@{0}", "Telephone"));
			  sqlBld.Append(", ");
			  sqlBld.Append(string.Format("{0}=@{0}", "Title"));
			  sqlBld.Append(", ");
			  sqlBld.Append(string.Format("{0}=@{0}", "HiredOn"));
			  return sqlBld.ToString();
			} 
		}

		protected override DbParameter[] CreateParameters(Employee entity)
		{
			DbParameter[] parameters = new DbParameter[]
			{ 
				dataAccess.CreateParameter(Employee.PropertyNames.FirstName, entity.FirstName),
				dataAccess.CreateParameter(Employee.PropertyNames.LastName, entity.LastName),
				dataAccess.CreateParameter(Employee.PropertyNames.EmailAddress, entity.EmailAddress),
				dataAccess.CreateParameter(Employee.PropertyNames.Telephone, entity.Telephone),
				dataAccess.CreateParameter(Employee.PropertyNames.Title, entity.Title),
				dataAccess.CreateParameter(Employee.PropertyNames.HiredOn, entity.HiredOn),
			};
			return parameters;
		}

		protected override Employee CreateEntityFromReader(DbDataReader reader)
		{
			Employee entity = new Employee();
			entity.FirstName =  reader.ReadString("FirstName");
			entity.LastName =  reader.ReadString("LastName");
			entity.EmailAddress =  reader.ReadString("EmailAddress");
			entity.Telephone =  reader.ReadString("Telephone");
			entity.Title =  reader.ReadString("Title");
			entity.HiredOn =  reader.ReadDateTime("HiredOn");
			return entity;
		}

		public override int Update(Employee entity)
		{
			return Update(entity, new QueryFilter[]{ new QueryFilter("(pk.ColumnName)", entity.(pk.PropertyName)) });
		}

		public override int UpdateBatch(IEnumerable<Employee> list)
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

		public override Employee SelectById(object id)
		{
			
		}  

		#endregion
	}
}

