///////////////////////////////////////////////////////////////////
//	
//	Auto generated -  Class Template 1.2.0
//
///////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace WebZoo.Data
{
	public partial class Animal : WebZoo.Data.BaseEntityInt, IEquatable<Animal>
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
		
		#region Equatable
		
		/// <summary>
        /// Determines whether the specified <see cref="Animal"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Animal"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Animal"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
		public bool Equals(Animal other)
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
            if (obj is Animal)
                return Equals((Animal)obj);
		
            return base.Equals(obj);
		}
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

