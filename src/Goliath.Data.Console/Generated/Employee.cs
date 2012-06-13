///////////////////////////////////////////////////////////////////
//	
//	Auto generated -  Class Template 1.2.0
//
///////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace WebZoo.Data
{
	public partial class Employee : WebZoo.Data.BaseEntityInt, IEquatable<Employee>
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
		IList<WebZoo.Data.Animal> animalsOnAnimalsHandler_EmployeeId = new List<WebZoo.Data.Animal>();
		public virtual IList<WebZoo.Data.Animal> AnimalsOnAnimalsHandler_EmployeeId { get{ return animalsOnAnimalsHandler_EmployeeId; } set{ animalsOnAnimalsHandler_EmployeeId = value; } }

		#endregion
		
		#region Equatable
		
		/// <summary>
        /// Determines whether the specified <see cref="Employee"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Employee"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Employee"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
		public bool Equals(Employee other)
		{
			if(other == null)
				return false;
			
			return other.Id.Equals(Id);
		}
		
		/// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
		public override int GetHashCode()
        {
        	return Id.GetHashCode();
        }
		
		/// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
		public override bool Equals(object obj)
        {
            if (obj is Employee)
                return Equals((Employee)obj);
		
            return base.Equals(obj);
		}
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

