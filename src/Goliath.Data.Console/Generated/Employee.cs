///////////////////////////////////////////////////////////////////
//	
//	Auto generated -  Class Template 1.1.0
//
///////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace WebZoo.Data
{
	public partial class Employee : WebZoo.Data.BaseEntityInt
	{
		#region Primary Key


		#endregion

		#region properties

		public virtual string FirstName { get; set; }
		public virtual string LastName { get; set; }
		public virtual string EmailAddress { get; set; }
		public virtual string Telephone { get; set; }
		public virtual string Title { get; set; }
		public virtual DateTime HiredOn { get; set; }
		public virtual int? AssignedToZooId { get; set; }

		#endregion

		#region relations

		public virtual WebZoo.Data.Zoo AssignedToZoo { get; set; }		
		public virtual IList<WebZoo.Data.Animal> AnimalsOnAnimalsHandler_EmployeeId { get; set; }

		#endregion
	}

	#region metadata

	public static partial class PropertyNames
	{
		/// <summary>
		/// Properties names for <see cref="WebZoo.Data.Employee"/>
		/// </summary>
		public static class Employee
		{
			public const string Id = "Id";
			public const string FirstName = "FirstName";
			public const string LastName = "LastName";
			public const string EmailAddress = "EmailAddress";
			public const string Telephone = "Telephone";
			public const string Title = "Title";
			public const string HiredOn = "HiredOn";
			public const string AssignedToZooId = "AssignedToZooId";
			public const string AssignedToZoo = "AssignedToZoo";
			public const string AnimalsOnAnimalsHandler_EmployeeId = "AnimalsOnAnimalsHandler_EmployeeId";
		}
	}

	#endregion
}

