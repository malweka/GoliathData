///////////////////////////////////////////////////////////////////
//	
//	Auto generated -  Class Template 1.1.0
//
///////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace WebZoo.Data
{
	public partial class Animal : WebZoo.Data.BaseEntityInt
	{
		#region Primary Key


		#endregion

		#region properties

		public virtual string Name { get; set; }
		public virtual double Age { get; set; }
		public virtual string Location { get; set; }
		public virtual DateTime ReceivedOn { get; set; }
		public virtual int ZooId { get; set; }

		#endregion

		#region relations

		public virtual WebZoo.Data.Zoo Zoo { get; set; }		
		public virtual IList<WebZoo.Data.Employee> EmployeesOnAnimalsHandler_AnimalId { get; set; }

		#endregion
	}

	#region metadata

	public static partial class PropertyNames
	{
		/// <summary>
		/// Properties names for <see cref="WebZoo.Data.Animal"/>
		/// </summary>
		public static class Animal
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
	}

	#endregion
}

