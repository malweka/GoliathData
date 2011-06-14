///////////////////////////////////////////////////////////////////
//	
//	Auto generated -  Class Template 1.1.0
//
///////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace WebZoo.Data.SqlServer
{
	public partial class Zoo : WebZoo.Data.BaseEntity
	{
		#region Primary Key


		#endregion

		#region properties

		public virtual string Name { get; set; }
		public virtual string City { get; set; }
		public virtual bool AcceptNewAnimals { get; set; }

		#endregion

		#region relations

		public virtual IList<WebZoo.Data.SqlServer.Animal> AnimalsOnZooId { get; set; }
		public virtual IList<WebZoo.Data.SqlServer.Employee> EmployeesOnAssignedToZooId { get; set; }

		#endregion

		#region metadata

		public struct PropertyNames
		{
			public const string Id = "Id";
			public const string Name = "Name";
			public const string City = "City";
			public const string AcceptNewAnimals = "AcceptNewAnimals";
			public const string AnimalsOnZooId = "AnimalsOnZooId";
			public const string EmployeesOnAssignedToZooId = "EmployeesOnAssignedToZooId";
		}

		#endregion
	}
}

