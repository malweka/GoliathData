///////////////////////////////////////////////////////////////////
//	
//	Auto generated -  Class Template 1.2.0
//
///////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace WebZoo.Data
{
	public partial class Task : WebZoo.Data.BaseEntity, IEquatable<Task>
	{
		#region Primary Key


		#endregion

		#region properties

		public virtual string Title { get; set; }
		public virtual string TaskDescription { get; set; }
		public virtual DateTime CreatedOn { get; set; }
		public virtual DateTime? CompletedOn { get; set; }
		public virtual Guid? AssignedToId { get; set; }

		#endregion

		#region relations

		public virtual WebZoo.Data.UserAccount AssignedTo { get; set; }		

		#endregion
		
		#region Equatable
		
		/// <summary>
        /// Determines whether the specified <see cref="Task"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Task"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Task"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
		public bool Equals(Task other)
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
        	var entObj = obj as Task;
        	return entObj != null && Equals(entObj);
		}
		
		#endregion
		
	}

	#region metadata

	public static partial class PropertyNames
	{
		/// <summary>
		/// Properties names for <see cref="WebZoo.Data.Task"/>
		/// </summary>
		public static class Task
		{
			public const string Id = "Id";
			public const string Title = "Title";
			public const string TaskDescription = "TaskDescription";
			public const string CreatedOn = "CreatedOn";
			public const string CompletedOn = "CompletedOn";
			public const string AssignedToId = "AssignedToId";
			public const string AssignedTo = "AssignedTo";
		}
	}

	#endregion
}

