///////////////////////////////////////////////////////////////////
//	
//	Auto generated -  Class Template 1.2.0
//
///////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace WebZoo.Data
{
	public partial class Monkey : WebZoo.Data.Animal, IEquatable<Monkey>
	{
		#region Primary Key


		#endregion

		#region properties

		public virtual  Family { get; set; }
		public virtual  CanDoTricks { get; set; }

		#endregion

		#region relations


		#endregion
		
		#region Equatable
		
		/// <summary>
        /// Determines whether the specified <see cref="Monkey"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Monkey"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Monkey"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
		public bool Equals(Monkey other)
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
            if (obj is Monkey)
                return Equals((Monkey)obj);
		
            return base.Equals(obj);
		}
		#endregion
		
	}

	#region metadata

	public static partial class PropertyNames
	{
		/// <summary>
		/// Properties names for <see cref="WebZoo.Data.Monkey"/>
		/// </summary>
		public static class Monkey
		{
			public const string Id = "Id";
			public const string Family = "Family";
			public const string CanDoTricks = "CanDoTricks";
		}
	}

	#endregion
}

