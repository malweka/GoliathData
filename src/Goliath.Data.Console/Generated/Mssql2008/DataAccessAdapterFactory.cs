using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace WebZoo.Data.SqlServer
{
	public partial class DataAccessAdapterFactory
	{
		readonly Dictionary<Type, Func<object>> adapters;
		IDbAccess dataAccess;

		public DataAccessAdapterFactory(IDbAccess dataAccess)
		{
			if(dataAccess == null)
				throw new ArgumentNullException("dataAccess");
			this.dataAccess = dataAccess;

			adapters = new Dictionary<Type, Func<object>>()
			{
				{typeof(Zoo), ()=>{return new ZooDataAdapter(dataAccess);}},
				{typeof(Animal), ()=>{return new AnimalDataAdapter(dataAccess);}},
				{typeof(Employee), ()=>{return new EmployeeDataAdapter(dataAccess);}},
				{typeof(AnimalsHandler), ()=>{return new AnimalsHandlerDataAdapter(dataAccess);}},
				{typeof(Monkey), ()=>{return new MonkeyDataAdapter(dataAccess);}},
			};
		}

		public DataAccessAdapter<T> CreateAdapter<T>() where T : class
		{
			Type t = typeof(T);
			Func<object> factMethod;
			if(adapters.TryGetValue(t, out factMethod))
			{
				return (DataAccessAdapter<T>)factMethod.Invoke();
			}

			return null;
		}
	}

	public static partial class DataAccessAdapterExtensions
	{
	}
}

