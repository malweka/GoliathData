///////////////////////////////////////////////////////////////////
//	
//	Auto generated -  Class Template 1.2.0
//
///////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace WebZoo.Data
{
	public partial class UserAccount : WebZoo.Data.BaseEntity, IEquatable<UserAccount>
	{
		#region Primary Key


		#endregion

		#region properties

		public virtual string UserName { get; set; }
		public virtual string EmailAddress { get; set; }
		public virtual DateTime? LastAccessOn { get; set; }
		public virtual DateTime AccountCreatedOn { get; set; }

		#endregion

		#region relations

		IList<WebZoo.Data.Task> tasksOnAssignedToId = new List<WebZoo.Data.Task>();
		public virtual IList<WebZoo.Data.Task> TasksOnAssignedToId { get { return tasksOnAssignedToId; } set { tasksOnAssignedToId = value; } }
		IList<WebZoo.Data.Role> rolesOnUserRole_UserId = new List<WebZoo.Data.Role>();
		public virtual IList<WebZoo.Data.Role> RolesOnUserRole_UserId { get { return rolesOnUserRole_UserId; } set { rolesOnUserRole_UserId = value; } }

		#endregion
		
		#region Equatable
		
		/// <summary>
        /// Determines whether the specified <see cref="UserAccount"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="UserAccount"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="UserAccount"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
		public bool Equals(UserAccount other)
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
            if (obj is UserAccount)
                return Equals((UserAccount)obj);
		
            return base.Equals(obj);
		}
		#endregion
		
	}

	#region metadata

	public static partial class PropertyNames
	{
		/// <summary>
		/// Properties names for <see cref="WebZoo.Data.UserAccount"/>
		/// </summary>
		public static class UserAccount
		{
			public const string Id = "Id";
			public const string UserName = "UserName";
			public const string EmailAddress = "EmailAddress";
			public const string LastAccessOn = "LastAccessOn";
			public const string AccountCreatedOn = "AccountCreatedOn";
			public const string TasksOnAssignedToId = "TasksOnAssignedToId";
			public const string RolesOnUserRole_UserId = "RolesOnUserRole_UserId";
		}
	}

	#endregion
}

