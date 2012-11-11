///////////////////////////////////////////////////////////////////
//	
//	Auto generated -  Class Template 1.2.0
//
///////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace WebZoo.Data
{
	public partial class Role : WebZoo.Data.BaseEntity, IEquatable<Role>
	{
		#region Primary Key


		#endregion

		#region properties

		public virtual string Name { get; set; }
		public virtual string Description { get; set; }

		#endregion

		#region relations

		IList<WebZoo.Data.UserAccount> userAccountsOnUserRole_RoleId = new List<WebZoo.Data.UserAccount>();
		public virtual IList<WebZoo.Data.UserAccount> UserAccountsOnUserRole_RoleId { get { return userAccountsOnUserRole_RoleId; } set { userAccountsOnUserRole_RoleId = value; } }

		#endregion
		
		#region Equatable
		
		/// <summary>
        /// Determines whether the specified <see cref="Role"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Role"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Role"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
		public bool Equals(Role other)
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
            if (obj is Role)
                return Equals((Role)obj);
		
            return base.Equals(obj);
		}
		#endregion
		
	}

	#region metadata

	public static partial class PropertyNames
	{
		/// <summary>
		/// Properties names for <see cref="WebZoo.Data.Role"/>
		/// </summary>
		public static class Role
		{
			public const string Id = "Id";
			public const string Name = "Name";
			public const string Description = "Description";
			public const string UserAccountsOnUserRole_RoleId = "UserAccountsOnUserRole_RoleId";
		}
	}

	#endregion
}

