///////////////////////////////////////////////////////////////////
//	
//	Auto generated -  Class Template 1.1.0
//
///////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace WebZoo.Data.SqlServer
{
	public partial class Monkey : WebZoo.Data.SqlServer.Animal
	{
		#region Primary Key


		#endregion

		#region properties

		public virtual string Family { get; set; }
		public virtual bool CanDoTricks { get; set; }

		#endregion

		#region relations


		#endregion
	}

	#region metadata
	
	public static class MonkeyPropertyNames
	{
		public const string Id = "Id";
		public const string Family = "Family";
		public const string CanDoTricks = "CanDoTricks";
	}

	#endregion
}

