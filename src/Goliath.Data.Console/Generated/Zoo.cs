///////////////////////////////////////////////////////////////////
//	
//	Auto generated -  Class Template 1.2.0
//
///////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace WebZoo.Data
{
	public partial class Zoo : WebZoo.Data.BaseEntityInt, IEquatable<Zoo>
	{
		#region Primary Key


		#endregion

		#region properties

		public virtual string Name { get; set; }
		public virtual string City { get; set; }
		public virtual bool AcceptNewAnimals { get; set; }

		#endregion

		#region relations

		IList<WebZoo.Data.Animal> animalsOnZooId = new List<WebZoo.Data.Animal>();
		public virtual IList<WebZoo.Data.Animal> AnimalsOnZooId { get { return animalsOnZooId; } set { animalsOnZooId = value; } }
		IList<WebZoo.Data.Employee> employeesOnAssignedToZooId = new List<WebZoo.Data.Employee>();
		public virtual IList<WebZoo.Data.Employee> EmployeesOnAssignedToZooId { get { return employeesOnAssignedToZooId; } set { employeesOnAssignedToZooId = value; } }

		#endregion
		
		#region Equatable
		
		/// <summary>
        /// Determines whether the specified <see cref="Zoo"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Zoo"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Zoo"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
		public bool Equals(Zoo other)
		{
			return other != null && other.Id.Equals(Id);
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
        	var entObj = obj as Zoo;
        	return entObj != null && Equals(entObj);
		}
		
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

