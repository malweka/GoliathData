///////////////////////////////////////////////////////////////////
//	
//	Auto generated -  Class Template 1.1.0
//
///////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace WebZoo.Data
{
	public partial class Zoo : WebZoo.Data.BaseEntityInt
	{
		#region Primary Key


		#endregion

		#region properties

		public virtual string Name { get; set; }
		public virtual string City { get; set; }
		public virtual bool AcceptNewAnimals { get; set; }

		#endregion

		#region relations

		public virtual IList<WebZoo.Data.Animal> AnimalsOnZooId { get; set; }
		public virtual IList<WebZoo.Data.Employee> EmployeesOnAssignedToZooId { get; set; }

		#endregion
	}

	#region metadata

	public static partial class PropertyNames
	{
		/// <summary>
		/// Properties names for <see cref="WebZoo.Data.Zoo"/>
		/// </summary>
		public static class Zoo
		{
			public const string Id = "Id";
			public const string Name = "Name";
			public const string City = "City";
			public const string AcceptNewAnimals = "AcceptNewAnimals";
			public const string AnimalsOnZooId = "AnimalsOnZooId";
			public const string EmployeesOnAssignedToZooId = "EmployeesOnAssignedToZooId";
		}
	}

	#endregion
}

