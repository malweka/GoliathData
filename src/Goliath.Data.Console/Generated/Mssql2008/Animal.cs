///////////////////////////////////////////////////////////////////
//	
//	Auto generated -  Class Template 1.1.0
//
///////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace WebZoo.Data.SqlServer
{
	public partial class Animal : WebZoo.Data.BaseEntity
	{
		#region Primary Key


		#endregion

		#region properties

		public virtual string Name { get; set; }
		public virtual float Age { get; set; }
		public virtual string Location { get; set; }
		public virtual DateTime ReceivedOn { get; set; }

		#endregion

		#region relations

		public virtual WebZoo.Data.SqlServer.Zoo Zoo { get; set; }
		public virtual Guid ZooId { get; set; }
		public virtual IList<WebZoo.Data.SqlServer.Employee> EmployeesOnAnimalsHandler_AnimalId { get; set; }

		#endregion

		#region metadata

		public struct PropertyNames
		{
			public const string Id = "Id";
			public const string Name = "Name";
			public const string Age = "Age";
			public const string Location = "Location";
			public const string ReceivedOn = "ReceivedOn";
			public const string ZooId = "ZooId";
			public const string Zoo = "Zoo";
			public const string EmployeesOnAnimalsHandler_AnimalId = "EmployeesOnAnimalsHandler_AnimalId";
		}

		#endregion
	}
}

