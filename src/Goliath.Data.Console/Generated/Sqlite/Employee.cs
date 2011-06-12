///////////////////////////////////////////////////////////////////
//	
//	Goliath.Data Generated Class -  Class Template 1.1.0
//
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace WebZoo.Data.Sqlite
{
	public partial class Employee : WebZoo.Data.BaseEntity
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

		#endregion

		#region relations

		public virtual WebZoo.Data.Sqlite.Zoo AssignedToZoo { get; set; }
		public virtual Guid? AssignedToZooId { get; set; }
		public virtual IList<WebZoo.Data.Sqlite.Animal> AnimalsOnAnimalsHandler_EmployeeId { get; set; }

		#endregion

		#region metadata

		public struct PropertyNames
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

		#endregion
	}
}

