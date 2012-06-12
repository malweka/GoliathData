 

//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Caching\Cache.cs
//------------------------------------------------------------------------------
#region License

// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/

#endregion


namespace Fasterflect.Caching
{
#if DOT_NET_4
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Diagnostics;
    
    [DebuggerStepThrough]
    internal sealed class Cache<TKey, TValue>
    {
        private readonly IDictionary<TKey, object> entries;

		#region Constructors
		public Cache()
		{
			entries = new ConcurrentDictionary<TKey, object>();
		}
		public Cache(IEqualityComparer<TKey> equalityComparer)
		{
			entries = new ConcurrentDictionary<TKey, object>(equalityComparer);
		}
		#endregion

		#region Properties
		/// <summary>
		/// Returns the number of entries currently stored in the cache. Accessing this property
		/// causes a check of all entries in the cache to ensure collected entries are not counted.
		/// </summary>
		public int Count
		{
			get { return ClearCollected(); }
		}
		#endregion

		#region Indexers
		/// <summary>
		/// Indexer for accessing or adding cache entries.
		/// </summary>
		public TValue this[TKey key]
		{
			get { return Get(key); }
			set { Insert(key, value, CacheStrategy.Temporary); }
		}

		/// <summary>
		/// Indexer for adding a cache item using the specified strategy.
		/// </summary>
		public TValue this[TKey key, CacheStrategy strategy]
		{
			set { Insert(key, value, strategy); }
		}
		#endregion

		#region Insert Methods
		/// <summary>
		/// Insert a collectible object into the cache.
		/// </summary>
		/// <param name="key">The cache key used to reference the item.</param>
		/// <param name="value">The object to be inserted into the cache.</param>
		public void Insert(TKey key, TValue value)
		{
			Insert(key, value, CacheStrategy.Temporary);
		}

		/// <summary>
		/// Insert an object into the cache using the specified cache strategy (lifetime management).
		/// </summary>
		/// <param name="key">The cache key used to reference the item.</param>
		/// <param name="value">The object to be inserted into the cache.</param>
		/// <param name="strategy">The strategy to apply for the inserted item (use Temporary for objects 
		/// that are collectible and Permanent for objects you wish to keep forever).</param>
		public void Insert(TKey key, TValue value, CacheStrategy strategy)
		{
			entries[key] = strategy == CacheStrategy.Temporary 
				? new WeakReference(value) 
				: value as object;
		}
		#endregion

		#region Get Methods
		/// <summary>
		/// Retrieves an entry from the cache using the given key.
		/// </summary>
		/// <param name="key">The cache key of the item to retrieve.</param>
		/// <returns>The retrieved cache item or null if not found.</returns>
		public TValue Get(TKey key)
		{
			object entry;
			entries.TryGetValue(key, out entry);
			var wr = entry as WeakReference;
			return (TValue)(wr != null ? wr.Target : entry);
		}
		#endregion

		#region Remove Methods
		/// <summary>
		/// Removes the object associated with the given key from the cache.
		/// </summary>
		/// <param name="key">The cache key of the item to remove.</param>
		/// <returns>True if an item removed from the cache and false otherwise.</returns>
		public bool Remove(TKey key)
		{
			return entries.Remove(key);
		}
		#endregion

		#region Clear Methods
		/// <summary>
		/// Removes all entries from the cache.
		/// </summary>
		public void Clear()
		{
			entries.Clear();
		}

		/// <summary>
		/// Process all entries in the cache and remove entries that refer to collected entries.
		/// </summary>
		/// <returns>The number of live cache entries still in the cache.</returns>
		private int ClearCollected()
		{
			IList<TKey> keys = entries.Where(kvp => kvp.Value is WeakReference && !(kvp.Value as WeakReference).IsAlive).Select(kvp => kvp.Key).ToList();
			keys.ForEach(k => entries.Remove(k));
			return entries.Count;
		}
		#endregion

		#region ToString
		/// <summary>
		/// This method returns a string with information on the cache contents (number of contained objects).
		/// </summary>
		public override string ToString()
		{
			int count = ClearCollected();
			return count > 0 ? String.Format("Cache contains {0} live objects.", count) : "Cache is empty.";
		}
		#endregion
    }
#elif DOT_NET_35
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Diagnostics;

    [DebuggerStepThrough]
	internal sealed class Cache<TKey,TValue>
	{
		private readonly Dictionary<TKey, object> entries;
		private int owner;

        #region Constructors
		public Cache()
		{
			entries = new Dictionary<TKey,object>();
		}
		public Cache( IEqualityComparer<TKey> equalityComparer )
		{
			entries = new Dictionary<TKey,object>( equalityComparer );
		}
        #endregion

        #region Properties
		/// <summary>
		/// Returns the number of entries currently stored in the cache. Accessing this property
		/// causes a check of all entries in the cache to ensure collected entries are not counted.
		/// </summary>
		public int Count
		{
			get { return ClearCollected(); }
		}
        #endregion

        #region Indexers
		/// <summary>
		/// Indexer for accessing or adding cache entries.
		/// </summary>
		public TValue this[ TKey key ]
		{
			get { return Get( key ); }
			set { Insert( key, value, CacheStrategy.Temporary ); }
		}

		/// <summary>
		/// Indexer for adding a cache item using the specified strategy.
		/// </summary>
		public TValue this[ TKey key, CacheStrategy strategy ]
		{
			set { Insert( key, value, strategy ); }
		}
        #endregion

    #region Insert Methods
		/// <summary>
		/// Insert a collectible object into the cache.
		/// </summary>
		/// <param name="key">The cache key used to reference the item.</param>
		/// <param name="value">The object to be inserted into the cache.</param>
		public void Insert( TKey key, TValue value )
		{
			Insert( key, value, CacheStrategy.Temporary );
		}

		/// <summary>
		/// Insert an object into the cache using the specified cache strategy (lifetime management).
		/// </summary>
		/// <param name="key">The cache key used to reference the item.</param>
		/// <param name="value">The object to be inserted into the cache.</param>
		/// <param name="strategy">The strategy to apply for the inserted item (use Temporary for objects 
		/// that are collectible and Permanent for objects you wish to keep forever).</param>
		public void Insert( TKey key, TValue value, CacheStrategy strategy )
		{
			object entry = strategy == CacheStrategy.Temporary ? new WeakReference( value ) : value as object;
			int current = Thread.CurrentThread.ManagedThreadId;
			while( Interlocked.CompareExchange( ref owner, current, 0 ) != current ) { }
			entries[ key ] = entry;
			if( current != Interlocked.Exchange( ref owner, 0 ) )
				throw new UnauthorizedAccessException( "Thread had access to cache even though it shouldn't have." );
		}
        #endregion

        #region GetValue Methods
		/// <summary>
		/// Retrieves an entry from the cache using the given key.
		/// </summary>
		/// <param name="key">The cache key of the item to retrieve.</param>
		/// <returns>The retrieved cache item or null if not found.</returns>
		public TValue Get( TKey key )
        {
			int current = Thread.CurrentThread.ManagedThreadId;
			while( Interlocked.CompareExchange( ref owner, current, 0 ) != current ) { }
			object entry;
			entries.TryGetValue( key, out entry );
			if( current != Interlocked.Exchange( ref owner, 0 ) )
				throw new UnauthorizedAccessException( "Thread had access to cache even though it shouldn't have." );
			var wr = entry as WeakReference;
			return (TValue) (wr != null ? wr.Target : entry);
		}
        #endregion

        #region Remove Methods
		/// <summary>
		/// Removes the object associated with the given key from the cache.
		/// </summary>
		/// <param name="key">The cache key of the item to remove.</param>
		/// <returns>True if an item removed from the cache and false otherwise.</returns>
		public bool Remove( TKey key )
		{
			int current = Thread.CurrentThread.ManagedThreadId;
			while( Interlocked.CompareExchange( ref owner, current, 0 ) != current ) { }
			bool found = entries.Remove( key );
			if( current != Interlocked.Exchange( ref owner, 0 ) )
				throw new UnauthorizedAccessException( "Thread had access to cache even though it shouldn't have." );
			return found;
		}
        #endregion

        #region Clear Methods
		/// <summary>
		/// Removes all entries from the cache.
		/// </summary>
		public void Clear()
		{
			int current = Thread.CurrentThread.ManagedThreadId;
			while( Interlocked.CompareExchange( ref owner, current, 0 ) != current ) { }
			entries.Clear();
			if( current != Interlocked.Exchange( ref owner, 0 ) )
				throw new UnauthorizedAccessException( "Thread had access to cache even though it shouldn't have." );
		}

		/// <summary>
		/// Process all entries in the cache and remove entries that refer to collected entries.
		/// </summary>
		/// <returns>The number of live cache entries still in the cache.</returns>
		private int ClearCollected()
		{
			int current = Thread.CurrentThread.ManagedThreadId;
			while( Interlocked.CompareExchange( ref owner, current, 0 ) != current ) { }
			IList<TKey> keys = entries.Where( kvp => kvp.Value is WeakReference && ! (kvp.Value as WeakReference).IsAlive ).Select( kvp => kvp.Key ).ToList();
			keys.ForEach( k => entries.Remove( k ) );
			int count = entries.Count;
			if( current != Interlocked.Exchange( ref owner, 0 ) )
				throw new UnauthorizedAccessException( "Thread had access to cache even though it shouldn't have." );
			return count;
		}
        #endregion

        #region ToString
        /// <summary>
		/// This method returns a string with information on the cache contents (number of contained objects).
		/// </summary>
		public override string ToString()
		{
			int count = ClearCollected();
			return count > 0 ? String.Format( "Cache contains {0} live objects.", count ) : "Cache is empty.";
		}
        #endregion
	}
#else
	#error At least one of the compilation symbols DOT_NET_4 or DOT_NET_35 must be defined. 
#endif
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Caching\CacheStrategy.cs
//------------------------------------------------------------------------------
#region License

// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/

#endregion

namespace Fasterflect.Caching
{
	/// <summary>
	/// An enumeration of the supported caching strategies.
	/// </summary>
	internal enum CacheStrategy
	{
		/// <summary>
		/// This value indicates that caching is disabled.
		/// </summary>
		None,
		/// <summary>
		/// This value indicates that caching is enabled, and that cached objects may be
		/// collected and released at will by the garbage collector. This is the default value. 
		/// </summary>
		Temporary,
		/// <summary>
		/// This value indicates that caching is enabled, and that cached objects may not
		/// be garbage collected. The developer must manually ensure that objects are 
		/// removed from the cache when they are no longer needed.
		/// </summary>
		Permanent
	}
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Common\Constants.cs
//------------------------------------------------------------------------------
#region License

// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/

#endregion


namespace Fasterflect
{
    using System;
    using System.Reflection;
    using Fasterflect.Emitter;

    internal static class Constants
    {
        public const string IndexerSetterName = "set_Item";
        public const string IndexerGetterName = "get_Item";
        public const string ArraySetterName = "[]=";
        public const string ArrayGetterName = "=[]";
        public static readonly Type ObjectType = typeof(object);
        public static readonly Type IntType = typeof(int);
        public static readonly Type StructType = typeof(ValueTypeHolder);
        public static readonly Type VoidType = typeof(void);
        public static readonly Type[] ArrayOfObjectType = new[] { typeof(object) };
        public static readonly object[] EmptyObjectArray = new object[0];
        public static readonly string[] EmptyStringArray = new string[0];
        public static readonly PropertyInfo[] EmptyPropertyInfoArray = new PropertyInfo[0];
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Common\Delegates.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion

namespace Fasterflect
{
    /// <summary>
    /// A delegate to retrieve the value of an instance field or property of an object.
    /// </summary>
    /// <param name="obj">The object whose field's or property's value is to be retrieved.</param>
    /// <returns>The value of the instance field or property.</returns>
    internal delegate object MemberGetter( object obj );

    /// <summary>
    /// A delegate to set the value of an instance field or property of an object.
    /// </summary>
    /// <param name="obj">The object whose field's or property's value is to be set.</param>
    /// <param name="value">The value to be set to the field or property.</param>
    internal delegate void MemberSetter( object obj, object value );

    /// <summary>
    /// A delegate to set an element of an array.
    /// </summary>
    /// <param name="array">The array whose element is to be set.</param>
    /// <param name="index">The index of the element to be set.</param>
    /// <param name="value">The value to set to the element.</param>
    internal delegate void ArrayElementSetter( object array, int index, object value );

    /// <summary>
    /// A delegate to retrieve an element of an array.
    /// </summary>
    /// <param name="array">The array whose element is to be retrieved</param>
    /// <param name="index">The index of the element to be retrieved</param>
    /// <returns>The element at <paramref name="index"/></returns>
    internal delegate object ArrayElementGetter( object array, int index );

    /// <summary>
    /// A delegate to invoke an instance method or indexer of an object.
    /// </summary>
    /// <param name="obj">The object whose method  or indexer is to be invoked on.</param>
    /// <param name="parameters">The properly-ordered parameter list of the method/indexer.  
    /// For indexer-set operation, the parameter array include parameters for the indexer plus
    /// the value to be set to the indexer.</param>
    /// <returns>The return value of the method or indexer.  Null is returned if the method has no
    /// return type or if it's a indexer-set operation.</returns>
    internal delegate object MethodInvoker( object obj, params object[] parameters );

    /// <summary>
    /// A delegate to invoke the constructor of a type.
    /// </summary>
    /// <param name="parameters">The properly-ordered parameter list of the constructor.</param>
    /// <returns>An instance of type whose constructor is invoked.</returns>
    internal delegate object ConstructorInvoker( params object[] parameters );

    /// <summary>
    /// A delegate to copy values of instance members (fields, properties, or both) from one object to another.
    /// </summary>
    /// <param name="source">The object whose instance members' values will be read.</param>
    /// <param name="target">The object whose instance members' values will be written.</param>
    internal delegate void ObjectMapper( object source, object target );
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Common\Flags.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

	/// <summary>
	/// This class encapsulates common <see cref="BindingFlags"/> combinations and provides various
	/// additional Fasterflect-specific flags to further tailor the lookup experience.
	/// </summary>
	internal struct Flags
	{
		private readonly long flags;
		private static readonly Dictionary<Flags, string> flagNames = new Dictionary<Flags, string>( 64 );

		#region Constructors
		private Flags( long flags )
		{
			this.flags = flags;
		}

		static Flags()
		{
			foreach( BindingFlags flag in Enum.GetValues( typeof(BindingFlags) ) )
			{
				if( flag != BindingFlags.Default )
				{
					flagNames[ new Flags( (long) flag ) ] = flag.ToString();
				}
			}
			flagNames[ PartialNameMatch ] = "PartialNameMatch"; // new Flags( 1L << 32 );
			flagNames[ TrimExplicitlyImplemented ] = "TrimExplicitlyImplemented"; // new Flags( 1L << 33 );
			flagNames[ ExcludeExplicitlyImplemented ] = "ExcludeExplicitlyImplemented"; // = new Flags( 1L << 34 );
			flagNames[ ExcludeBackingMembers ] = "ExcludeBackingMembers"; // = new Flags( 1L << 35 );
			flagNames[ IgnoreParameterModifiers ] = "IgnoreParameterModifiers"; // = new Flags( 1L << 36 );

			// not yet supported:
			//flagNames[ VisibilityMatch ] = "VisibilityMatch"; // = new Flags( 1L << 55 );
			//flagNames[ Private ] = "Private"; //   = new Flags( 1L << 56 );
			//flagNames[ Protected ] = "Protected"; // = new Flags( 1L << 57 );
			//flagNames[ Internal ] = "Internal"; //  = new Flags( 1L << 58 );

			//flagNames[ ModifierMatch ] = "ModifierMatch"; // = new Flags( 1L << 59 );
			//flagNames[ Abstract ] = "Abstract"; //  = new Flags( 1L << 60 );
			//flagNames[ Virtual ] = "Virtual"; //   = new Flags( 1L << 61 );
			//flagNames[ Override ] = "Override"; //  = new Flags( 1L << 62 );
			//flagNames[ New ] = "New"; //      = new Flags( 1L << 63 );
		}
		#endregion

		#region Flags Selectors

		#region BindingFlags
		/// <summary>
		/// This value corresponds to the <see href="BindingFlags.Default"/> value.
		/// </summary>
		public static readonly Flags None = new Flags( (long) BindingFlags.Default );

		/// <summary>
		/// This value corresponds to the <see href="BindingFlags.IgnoreCase"/> value.
		/// </summary>
		public static readonly Flags IgnoreCase = new Flags( (long) BindingFlags.IgnoreCase );

		/// <summary>
		/// This value corresponds to the <see href="BindingFlags.DeclaredOnly"/> value.
		/// </summary>
		public static readonly Flags DeclaredOnly = new Flags( (long) BindingFlags.DeclaredOnly );

		/// <summary>
		/// This value corresponds to the <see href="BindingFlags.ExactBinding"/> value. 
		/// Note that this value is respected even in cases where normal Reflection calls would ignore it.
		/// </summary>
		public static readonly Flags ExactBinding = new Flags( (long) BindingFlags.ExactBinding );

		/// <summary>
		/// This value corresponds to the <see href="BindingFlags.Public"/> value.
		/// </summary>
		public static readonly Flags Public = new Flags( (long) BindingFlags.Public );

		/// <summary>
		/// This value corresponds to the <see href="BindingFlags.NonPublic"/> value.
		/// </summary>
		public static readonly Flags NonPublic = new Flags( (long) BindingFlags.NonPublic );

		/// <summary>
		/// This value corresponds to the <see href="BindingFlags.Instance"/> value.
		/// </summary>
		public static readonly Flags Instance = new Flags( (long) BindingFlags.Instance );

		/// <summary>
		/// This value corresponds to the <see href="BindingFlags.Static"/> value.
		/// </summary>
		public static readonly Flags Static = new Flags( (long) BindingFlags.Static );
		#endregion

		#region FasterflectFlags
		/// <summary>
		/// If this option is specified the search for a named member will perform a partial match instead
		/// of an exact match. If <see href="TrimExplicitlyImplemented"/> is specified the trimmed name is
		/// used instead of the original member name. If <see href="IgnoreCase"/> is specified the 
		/// comparison uses <see href="StringComparison.OrginalIgnoreCase"/> and otherwise
		/// uses <see href="StringComparison.Ordinal"/>.
		/// </summary>
		public static readonly Flags PartialNameMatch = new Flags( 1L << 32 );

		/// <summary>
		/// If this option is specified the search for a named member will strip off the namespace and
		/// interface name from explicitly implemented interface members before applying any comparison
		/// operations.
		/// </summary>
		public static readonly Flags TrimExplicitlyImplemented = new Flags( 1L << 33 );

		/// <summary>
		/// If this option is specified the search for members will exclude explicitly implemented
		/// interface members.
		/// </summary>
		public static readonly Flags ExcludeExplicitlyImplemented = new Flags( 1L << 34 );

		/// <summary>
		/// If this option is specified all members that are backers for another member, such as backing
		/// fields for automatic properties or get/set methods for properties, will be excluded from the 
		/// result.
		/// </summary>
		public static readonly Flags ExcludeBackingMembers = new Flags( 1L << 35 );

		/// <summary>
		/// If this option is specified the search for methods will avoid checking whether parameters
		/// have been declared as ref or out. This allows you to locate a method by its signature
		/// without supplying the exact details for every parameter.
		/// </summary>
		public static readonly Flags IgnoreParameterModifiers = new Flags( 1L << 36 );
		

		#region For The Future
		///// <summary>
		///// If this option is specified only members with one (or more) of the specified visibility 
		///// flags will be included in the result.
		///// </summary>
		//public static readonly Flags VisibilityMatch = new Flags( 1L << 55 );
		///// <summary>
		///// Visibility flags
		///// </summary>
		//public static readonly Flags Private   = new Flags( 1L << 56 );
		//public static readonly Flags Protected = new Flags( 1L << 57 );
		//public static readonly Flags Internal  = new Flags( 1L << 58 );

		///// <summary>
		///// If this option is specified only members with one (or more) of the specified modifier 
		///// flags will be included in the result.
		///// </summary>
		//public static readonly Flags ModifierMatch = new Flags( 1L << 59 );
		///// <summary>
		///// Modifier flags
		///// </summary>
		//public static readonly Flags Abstract  = new Flags( 1L << 60 );
		//public static readonly Flags Virtual   = new Flags( 1L << 61 );
		//public static readonly Flags Override  = new Flags( 1L << 62 );
		//public static readonly Flags New       = new Flags( 1L << 63 );
		#endregion

		#endregion

		#region Common Selections
		/// <summary>
		/// Search criteria encompassing all public and non-public members, including base members.
		/// </summary>
		public static readonly Flags AnyVisibility = Public | NonPublic;

		/// <summary>
		/// Search criteria encompassing all public and non-public instance members, including base members.
		/// </summary>
		public static readonly Flags InstanceAnyVisibility = AnyVisibility | Instance;

		/// <summary>
		/// Search criteria encompassing all public and non-public static members, including base members.
		/// </summary>
		public static readonly Flags StaticAnyVisibility = AnyVisibility | Static;

		/// <summary>
		/// Search criteria encompassing all public and non-public instance members, excluding base members.
		/// </summary>
		public static readonly Flags InstanceAnyDeclaredOnly = InstanceAnyVisibility | DeclaredOnly;

		/// <summary>
		/// Search criteria encompassing all public and non-public static members, excluding base members.
		/// </summary>
		public static readonly Flags StaticAnyDeclaredOnly = StaticAnyVisibility | DeclaredOnly;

		/// <summary>
		/// Search criteria encompassing all members, including base members.
		/// </summary>
		public static readonly Flags StaticInstanceAnyVisibility = InstanceAnyVisibility | Static;
		#endregion

		#region Intellisense Convenience Flags
		/// <summary>
		/// Search criteria encompassing all public and non-public instance members, including base members.
		/// </summary>
		public static readonly Flags Default = InstanceAnyVisibility;

		/// <summary>
		/// Search criteria encompassing all members (public and non-public, instance and static), including base members.
		/// </summary>
		public static readonly Flags AllMembers = StaticInstanceAnyVisibility;
		#endregion

		#endregion

		#region Helper Methods
		/// <summary>
		/// Returns true if all values in the given <paramref name="mask"/> are set in the current Flags instance.
		/// </summary>
		public bool IsSet( BindingFlags mask )
		{
			return ((BindingFlags) flags & mask) == mask;
		}

		/// <summary>
		/// Returns true if all values in the given <paramref name="mask"/> are set in the current Flags instance.
		/// </summary>
		public bool IsSet( Flags mask )
		{
			return (flags & mask) == mask;
		}

		/// <summary>
		/// Returns true if at least one of the values in the given <paramref name="mask"/> are set in the current Flags instance.
		/// </summary>
		public bool IsAnySet( BindingFlags mask )
		{
			return ((BindingFlags) flags & mask) != 0;
		}

		/// <summary>
		/// Returns true if at least one of the values in the given <paramref name="mask"/> are set in the current Flags instance.
		/// </summary>
		public bool IsAnySet( Flags mask )
		{
			return (flags & mask) != 0;
		}

		/// <summary>
		/// Returns true if all values in the given <paramref name="mask"/> are not set in the current Flags instance.
		/// </summary>
		public bool IsNotSet( BindingFlags mask )
		{
			return ((BindingFlags) flags & mask) == 0;
		}

		/// <summary>
		/// Returns true if all values in the given <paramref name="mask"/> are not set in the current Flags instance.
		/// </summary>
		public bool IsNotSet( Flags mask )
		{
			return (flags & mask) == 0;
		}

		/// <summary>
		/// Returns a new Flags instance with the union of the values from <paramref name="flags"/> and 
		/// <paramref name="mask"/> if <paramref name="condition"/> is true, and otherwise returns the
		/// supplied <paramref name="flags"/>.
		/// </summary>
		public static Flags SetIf( Flags flags, Flags mask, bool condition )
		{
			return condition ? flags | mask : flags;
		}

		/// <summary>
		/// Returns a new Flags instance with the union of the values from <paramref name="flags"/> and 
		/// <paramref name="mask"/> if <paramref name="condition"/> is true, and otherwise returns a new 
		/// Flags instance with the values from <paramref name="flags"/> that were not in <paramref name="mask"/>.
		/// </summary>
		public static Flags SetOnlyIf( Flags flags, Flags mask, bool condition )
		{
			return condition ? flags | mask : (Flags) (flags & ~mask);
		}

		/// <summary>
		/// Returns a new Flags instance returns a new Flags instance with the values from <paramref name="flags"/> 
		/// that were not in <paramref name="mask"/> if <paramref name="condition"/> is true, and otherwise returns
		/// the supplied <paramref name="flags"/>.
		/// </summary>
		public static Flags ClearIf( Flags flags, Flags mask, bool condition )
		{
			return condition ? (Flags) (flags & ~mask) : flags;
		}
		#endregion

		#region Equals
		/// <summary>
		/// Compares the current Flags instance to the given <paramref name="obj"/>.
		/// Returns true only if <paramref name="obj"/> is a Flags instance representing an identical selection.
		/// </summary>
		public override bool Equals( object obj )
		{
			return obj != null && obj.GetType() == typeof(Flags) && flags == ((Flags) obj).flags;
		}

		/// <summary>
		/// Produces a unique hash code for the current Flags instance.
		/// </summary>
		public override int GetHashCode()
		{
			return flags.GetHashCode();
		}
		#endregion

		#region Operators
		/// <summary>
		/// Produces a new Flags instance with the values from <paramref name="f1"/> that were not in <paramref name="f2"/>.
		/// </summary>
		public static Flags operator -( Flags f1, Flags f2 )
		{
			return new Flags( f1.flags & ~f2.flags );
		}

		/// <summary>
		/// Produces a new Flags instance with the values from the union of <paramref name="f1"/> and <paramref name="f2"/>.
		/// </summary>
		public static Flags operator |( Flags f1, Flags f2 )
		{
			return new Flags( f1.flags | f2.flags );
		}

		/// <summary>
		/// Produces a new Flags instance with the values from the intersection of <paramref name="f1"/> and <paramref name="f2"/>.
		/// </summary>
		public static Flags operator &( Flags f1, Flags f2 )
		{
			return new Flags( f1.flags & f2.flags );
		}

		/// <summary>
		/// Compares two Flags instances and returns true if they represent identical selections.
		/// </summary>
		public static bool operator ==( Flags f1, Flags f2 )
		{
			return f1.flags == f2.flags;
		}

		/// <summary>
		/// Compares two Flags instances and returns true if they represent different selections.
		/// </summary>
		public static bool operator !=( Flags f1, Flags f2 )
		{
			return f1.flags != f2.flags;
		}
		#endregion

		#region Conversion Operators
		/// <summary>
		/// Converts from BindingFlags to Flags.
		/// </summary>
		public static implicit operator Flags( BindingFlags m )
		{
			return new Flags( (long) m );
		}

		/// <summary>
		/// Converts from long to Flags.
		/// </summary>
		public static explicit operator Flags( long m )
		{
			return new Flags( m );
		}

		/// <summary>
		/// Converts from Flags to BindingFlags.
		/// </summary>
		public static implicit operator BindingFlags( Flags m )
		{
			return (BindingFlags) m.flags;
		}

		/// <summary>
		/// Converts from Flags to long.
		/// </summary>
		public static implicit operator long( Flags m )
		{
			return m.flags;
		}
		#endregion

		#region ToString
		/// <summary>
		/// Returns a string representation of the Flags values selected by the current instance.
		/// </summary>
		public override string ToString()
		{
			Flags @this = this;
			List<string> names = flagNames.Where( kvp => @this.IsSet( kvp.Key ) )
										  .Select( kvp => kvp.Value )
										  .OrderBy( n => n ).ToList();
			int index = 0;
			var sb = new StringBuilder();
			names.ForEach( n => sb.AppendFormat( "{0}{1}", n, ++index < names.Count ? " | " : "" ) );
			return sb.ToString();
		}
		#endregion
	}
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Common\FormatOptions.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect
{
    using System;

	/// <summary>
	/// This enumeration allows you to customize the XML output of the ToXml extensions.
	/// </summary>
	[Flags]
	internal enum FormatOptions
	{
		/// <summary>
		/// This option specifies the empty set of options and does not affect the output.
		/// </summary>
		None = 0,
		/// <summary>
		/// If this option is specified the generated XML will include an XML document header.
		/// </summary>
		AddHeader = 1,
		/// <summary>
		/// If this option is specified a line feed will be emitted after every XML element.
		/// </summary>
		NewLineAfterElement = 2,
		/// <summary>
		/// If this option is specified nested tags will be indented either 1 tab character
		/// (the default) or 4 space characters.
		/// </summary>
		Indent = 4,
		/// <summary>
		/// If this option is specified indentation will use spaces instead of tabs.
		/// </summary>
		UseSpaces = 8,
		/// <summary>
		/// This option, which combines AddHeader, NewLineAfterElement and Indent, provides the 
		/// default set of options used. 
		/// </summary>
		Default = AddHeader | NewLineAfterElement | Indent
	}
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Common\MemberFilter.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2009 Buu Nguyen (http://www.buunguyen.net/blog)
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class MemberFilter
    {
        public static bool IsReservedName( this string name )
        {
            name = name.ToLowerInvariant();
            return name == ".ctor" || name == ".cctor";
        }

        public static string TrimExplicitlyImplementedName( this string name )
        {
            int index = name.IsReservedName() ? -1 : name.LastIndexOf( '.' ) + 1;
            return index > 0 ? name.Substring( index ) : name;
        }

        /// <summary>
        /// This method applies name filtering to a set of members.
        /// </summary>
        public static IList<T> Filter<T>( this IList<T> members, Flags bindingFlags, string[] names )
            where T : MemberInfo
        {
            var result = new List<T>( members.Count );
            bool ignoreCase = bindingFlags.IsSet( Flags.IgnoreCase );
            bool isPartial = bindingFlags.IsSet( Flags.PartialNameMatch );
            bool trimExplicit = bindingFlags.IsSet( Flags.TrimExplicitlyImplemented );

            for( int i = 0; i < members.Count; i++ )
            {
                var member = members[ i ];
                var memberName = trimExplicit ? member.Name.TrimExplicitlyImplementedName() : member.Name;
                for( int j = 0; j < names.Length; j++ )
                {
                    var name = names[ j ];
                	var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                    bool match = isPartial ? memberName.Contains( name ) : memberName.Equals( name, comparison );
                    if( match )
                    {
						result.Add( member );
                    	break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// This method applies type parameter type filtering to a set of methods.
        /// </summary>
        public static IList<T> Filter<T>(this IList<T> methods, Type[] genericTypes)
            where T : MethodBase
        {
            var result = new List<T>(methods.Count);
            for (int i = 0; i < methods.Count; i++)
            {
                var method = methods[i];
                if (method.ContainsGenericParameters)
                {
                    var genericArgs = method.GetGenericArguments();
                    if (genericArgs.Length != genericTypes.Length)
                        continue; 
                    result.Add(method);
                }
            }
            return result;
        }

        /// <summary>
        /// This method applies method parameter type filtering to a set of methods.
        /// </summary>
        public static IList<T> Filter<T>( this IList<T> methods, Flags bindingFlags, Type[] paramTypes )
            where T : MethodBase
        {
            var result = new List<T>( methods.Count );

            bool exact = bindingFlags.IsSet( Flags.ExactBinding );
            for( int i = 0; i < methods.Count; i++ )
            {
                var method = methods[ i ];
				// verify parameters
            	var parameters = method.GetParameters();
                if( parameters.Length != paramTypes.Length )
                {
                    continue;
                }
				// verify parameter type compatibility
                bool match = true;
                for( int j = 0; j < paramTypes.Length; j++ )
                {
                    var type = paramTypes[ j ];
                    var parameter = parameters[ j ];
                	Type parameterType = parameter.ParameterType;
                	bool ignoreParameterModifiers = ! exact;
					if( ignoreParameterModifiers && parameterType.IsByRef )
					{
						string name = parameterType.FullName;
						parameterType = Type.GetType( name.Substring( 0, name.Length - 1 ) ) ?? parameterType;
					}
                    match &= parameterType.IsGenericParameter || parameterType.ContainsGenericParameters || (exact ? type == parameterType : parameterType.IsAssignableFrom( type ));
                    if( ! match )
                    {
                        break;
                    }
                }
				if( match )
				{
	                result.Add( method );
				}
            }
            return result;
        }

        /// <summary>
        /// This method applies member type filtering to a set of members.
        /// </summary>
        public static IList<T> Filter<T>( this IList<T> members, Flags bindingFlags, MemberTypes memberTypes )
            where T : MemberInfo
        {
            var result = new List<T>( members.Count );

            for( int i = 0; i < members.Count; i++ )
            {
                var member = members[ i ];
                bool match = (member.MemberType & memberTypes) == member.MemberType;
                if( ! match )
                {
					continue;
                }
                result.Add( member );
            }
            return result;
        }

        /// <summary>
        /// This method applies flags-based filtering to a set of members.
        /// </summary>
        public static IList<T> Filter<T>( this IList<T> members, Flags bindingFlags ) where T : MemberInfo
        {
            var result = new List<T>( members.Count );
        	var properties = new List<string>( members.Count );

            for( int i = 0; i < members.Count; i++ )
            {
                var member = members[ i ];

                bool excludeBacking = bindingFlags.IsSet( Flags.ExcludeBackingMembers );
                bool excludeExplicit = bindingFlags.IsSet( Flags.ExcludeExplicitlyImplemented );

            	bool exclude = false;
				if( excludeBacking )
				{
					exclude |= member is FieldInfo && member.Name[ 0 ] == '<';
					var method = member as MethodInfo;
 					if( method != null )
 					{
 						// filter out property backing methods
						exclude |= member.Name.Length > 4 && member.Name.Substring( 1, 3 ) == "et_";
						// filter out base implementations when an overrride exists
						exclude |= result.ContainsOverride( method );
 					}
					var property = member as PropertyInfo;
					if( property != null )
					{
						MethodInfo propertyGetter = property.GetGetMethod( true );
						exclude |= propertyGetter.IsVirtual && properties.Contains( property.Name );
						if( ! exclude )
						{
							properties.Add( property.Name );
						}
					}
				}
                exclude |= excludeExplicit && member.Name.Contains( "." ) && ! member.Name.IsReservedName();
                if( exclude )
                {
					continue;
                }
                result.Add( member );
            }
            return result;
        }

		private static bool ContainsOverride<T>( this IList<T> candidates, MethodInfo method ) where T : MemberInfo
    	{
			if( ! method.IsVirtual )
				return false;
   			var parameters = method.Parameters();
			for( int i = 0; i < candidates.Count; i++ )
			{
				MethodInfo candidate = candidates[ i ] as MethodInfo;
				if( candidate == null || ! candidate.IsVirtual || method.Name != candidate.Name )
				{
					continue;
				}
				if( parameters.Select( p => p.ParameterType ).SequenceEqual( candidate.Parameters().Select( p => p.ParameterType ) ) )
				{
					return true;
				}
			}
			return false;
    	}
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Common\Utils.cs
//------------------------------------------------------------------------------
#region License

// Copyright 2009 Buu Nguyen (http://www.buunguyen.net/blog)
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/

#endregion


namespace Fasterflect
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using Fasterflect.Emitter;

    [DebuggerStepThrough]
	internal static class Utils
	{
		public static Type GetTypeAdjusted( this object obj )
		{
			var wrapper = obj as ValueTypeHolder;
			return wrapper == null
				? obj is Type ? obj as Type : obj.GetType()
			    : wrapper.Value.GetType();
		}

        public static Type[] ToTypeArray(this ParameterInfo[] parameters)
        {
            if (parameters.Length == 0)
                return Type.EmptyTypes;
            var types = new Type[parameters.Length];
            for (int i = 0; i < types.Length; i++)
            {
                types[i] = parameters[i].ParameterType;
            }
            return types;
        }

		public static Type[] ToTypeArray(this object[] objects)
        {
            if (objects.Length == 0)
                return Type.EmptyTypes;
			var types = new Type[objects.Length];
			for (int i = 0; i < types.Length; i++)
			{
				var obj = objects[ i ];
				types[i] = obj != null ? obj.GetType() : null;
			}
			return types;
		}

		public static void ForEach<T>( this IEnumerable<T> source, Action<T> action )
		{
			foreach( T element in source )
			{
				action( element );
			}
		}
	}
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\DynamicReflection\DynamicBuilder.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion

#if DOT_NET_4

namespace Fasterflect
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;

	internal sealed class DynamicBuilder : DynamicObject
	{
		private readonly Dictionary <string, object> members = new Dictionary <string, object>();  
		
		#region DynamicObject Overrides
		/// <summary>
		/// Assigns the given value to the specified member, overwriting any previous definition if one existed.
		/// </summary>     
		public override bool TrySetMember( SetMemberBinder binder, object value )
		{
	        members[ binder.Name ] = value;
		    return true;
		}

		/// <summary>
		/// Gets the value of the specified member.
		/// </summary>      
		public override bool TryGetMember( GetMemberBinder binder, out object result )
		{
		    if( members.ContainsKey( binder.Name ) )
		    {
		        result = members[ binder.Name ];
		        return true;
		    }
		    return base.TryGetMember( binder, out result );
		}

		/// <summary>
		/// Invokes the specified member (if it is a delegate).
		/// </summary>     
		public override bool TryInvokeMember( InvokeMemberBinder binder, object[] args, out object result )
		{
			object member;
			if( members.TryGetValue( binder.Name, out member ) )
		    {
		    	var method = member as Delegate;
				if( method != null )
				{
					result = method.DynamicInvoke( args );
			        return true;
				}
		    }
		    return base.TryInvokeMember( binder, args, out result );
		}

		/// <summary>
		/// Gets a list of all dynamically defined members.
		/// </summary>
		public override IEnumerable<string> GetDynamicMemberNames()
		{
		    return members.Keys;
		}
		#endregion
	}
}
#endif


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\DynamicReflection\DynamicWrapper.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion

#if DOT_NET_4

namespace Fasterflect
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;

	internal sealed class DynamicWrapper : DynamicObject
	{
		private readonly object target;

		#region Constructors
		public DynamicWrapper( object target )
		{
			this.target = target;
		}
		public DynamicWrapper( ref ValueType target )
		{
			this.target = target.WrapIfValueType();
		}
		#endregion

		#region DynamicObject Overrides
		/// <summary>
		/// Sets the member on the target to the given value. Returns true if the value was
		/// actually written to the underlying member.
		/// </summary>     
		public override bool TrySetMember( SetMemberBinder binder, object value )
		{
			return target.TrySetValue( binder.Name, value );
		}

		/// <summary>
		/// Gets the member on the target and assigns it to the result parameter. Returns
		/// true if a value other than null was found and false otherwise.
		/// </summary>      
		public override bool TryGetMember( GetMemberBinder binder, out object result )
		{
			result = target.TryGetValue( binder.Name );
			return result != null;
		}

		/// <summary>
		/// Invokes the method specified and assigns the result to the result parameter. Returns
		/// true if a method to invoke was found and false otherwise.
		/// </summary>     
		public override bool TryInvokeMember( InvokeMemberBinder binder, object[] args, out object result )
		{
			var bindingFlags = Flags.InstanceAnyVisibility | Flags.IgnoreParameterModifiers;
			var method = target.GetType().Method( binder.Name, args.ToTypeArray(), bindingFlags );
			result = method == null ? null : method.Call( target, args );
			return method != null;
		}

		/// <summary>
		/// Gets all member names from the underlying instance.
		/// </summary>
		public override IEnumerable<string> GetDynamicMemberNames()
		{
			return target.GetType().Members().Select( m => m.Name );
		}
		#endregion
	}
}
#endif


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Emitter\ArrayGetEmitter.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect.Emitter
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    internal class ArrayGetEmitter : BaseEmitter
    {
        public ArrayGetEmitter( Type targetType )
            : base(new CallInfo( targetType, null, Flags.InstanceAnyVisibility, MemberTypes.Method,
                                     Constants.ArrayGetterName, new[] { typeof(int) }, null, true ))
        {
        }

        protected internal override DynamicMethod CreateDynamicMethod()
        {
            return CreateDynamicMethod( Constants.ArrayGetterName, CallInfo.TargetType,
                                        Constants.ObjectType, new[] { Constants.ObjectType, Constants.IntType } );
        }

        protected internal override Delegate CreateDelegate()
        {
            Type elementType = CallInfo.TargetType.GetElementType();
            Generator.ldarg_0 // load array
                .castclass( CallInfo.TargetType ) // (T[])array
                .ldarg_1 // load index
                .ldelem( elementType ) // load array[index]
                .boxIfValueType( elementType ) // [box] return
                .ret();
            return Method.CreateDelegate( typeof(ArrayElementGetter) );
        }
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Emitter\ArraySetEmitter.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect.Emitter
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    internal class ArraySetEmitter : BaseEmitter
    {
        public ArraySetEmitter( Type targetType )
            : base(new CallInfo(targetType, null, Flags.InstanceAnyVisibility, MemberTypes.Method, Constants.ArraySetterName,
                                     new[] { typeof(int), targetType.GetElementType() }, null, false))
        {
        }

        protected internal override DynamicMethod CreateDynamicMethod()
        {
            return CreateDynamicMethod( Constants.ArraySetterName, CallInfo.TargetType, null,
                                        new[] { Constants.ObjectType, Constants.IntType, Constants.ObjectType } );
        }

        protected internal override Delegate CreateDelegate()
        {
            Type elementType = CallInfo.TargetType.GetElementType();
            Generator.ldarg_0 // load array
                .castclass( CallInfo.TargetType ) // (T[])array
                .ldarg_1 // load index
                .ldarg_2 // load value
                .CastFromObject( elementType ) // (unbox | cast) value
                .stelem( elementType ) // array[index] = value
                .ret();
            return Method.CreateDelegate( typeof(ArrayElementSetter) );
        }
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Emitter\BaseEmitter.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect.Emitter
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using Fasterflect.Caching;

    internal abstract class BaseEmitter
    {
        private static readonly Cache<CallInfo, Delegate> cache = new Cache<CallInfo, Delegate>();
        protected static readonly MethodInfo StructGetMethod =
            Constants.StructType.GetMethod("get_Value", BindingFlags.Public | BindingFlags.Instance);
        protected static readonly MethodInfo StructSetMethod =
            Constants.StructType.GetMethod("set_Value", BindingFlags.Public | BindingFlags.Instance);

        protected CallInfo CallInfo;
        protected DynamicMethod Method;
        protected EmitHelper Generator;

        protected BaseEmitter(CallInfo callInfo)
        {
            CallInfo = callInfo;
        }
        
        internal Delegate GetDelegate()
        {
        	var action = cache.Get( CallInfo );
			if( action == null )
			{
				Method = CreateDynamicMethod();
				Generator = new EmitHelper( Method.GetILGenerator() );
				action = CreateDelegate();
				cache.Insert( CallInfo, action, CacheStrategy.Temporary );
			}
			return action;
        }

        protected internal abstract DynamicMethod CreateDynamicMethod();
        protected internal abstract Delegate CreateDelegate();

        protected internal static DynamicMethod CreateDynamicMethod( string name, Type targetType, Type returnType,
                                                                     Type[] paramTypes )
        {
            return new DynamicMethod( name, MethodAttributes.Static | MethodAttributes.Public,
                                      CallingConventions.Standard, returnType, paramTypes,
                                      targetType.IsArray ? targetType.GetElementType() : targetType,
                                      true );
        }

        protected void LoadInnerStructToLocal( byte localPosition )
        {
            Generator
                .castclass( Constants.StructType ) // (ValueTypeHolder)wrappedStruct
                .callvirt(StructGetMethod) // <stack>.get_Value()
                .unbox_any( CallInfo.TargetType ) // unbox <stack>
                .stloc( localPosition ) // localStr = <stack>
                .ldloca_s( localPosition ); // load &localStr
        }

        protected void StoreLocalToInnerStruct( byte localPosition )
        {
            StoreLocalToInnerStruct( 0, localPosition ); // 0: 'this'
        }

        protected void StoreLocalToInnerStruct(byte argPosition, byte localPosition)
        {
            Generator
                .ldarg(argPosition)
                .castclass(Constants.StructType) // wrappedStruct = (ValueTypeHolder)this
                .ldloc(localPosition) // load localStr
                .boxIfValueType(CallInfo.TargetType) // box <stack>
                .callvirt(StructSetMethod); // wrappedStruct.set_Value(<stack>)
        }
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Emitter\CallInfo.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect.Emitter
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Stores all necessary information to construct a dynamic method.
    /// </summary>
    [DebuggerStepThrough]
    internal class CallInfo
    {
        public Type TargetType { get; private set; }
        public Flags BindingFlags { get; internal set; }
        public MemberTypes MemberTypes { get; set; }
        public Type[] ParamTypes { get; internal set; }
        public Type[] GenericTypes { get; private set; }
        public string Name { get; private set; }
        public bool IsReadOperation { get; set; }
        public bool IsStatic { get; internal set; }
		
        // This field doesn't constitute CallInfo identity:
        public MemberInfo MemberInfo { get; internal set; }

        public CallInfo(Type targetType, Type[] genericTypes, Flags bindingFlags, MemberTypes memberTypes, string name,
                        Type[] parameterTypes, MemberInfo memberInfo, bool isReadOperation )
        {
            TargetType = targetType;
            GenericTypes = genericTypes == null || genericTypes.Length == 0
                             ? Type.EmptyTypes
                             : genericTypes;
            BindingFlags = bindingFlags;
            MemberTypes = memberTypes;
            Name = name;
            ParamTypes = parameterTypes == null || parameterTypes.Length == 0
                             ? Type.EmptyTypes
                             : parameterTypes;
            MemberInfo = memberInfo;
        	IsReadOperation = isReadOperation;
        	IsStatic = BindingFlags.IsSet( Flags.Static );
        }

        /// <summary>
        /// The CIL should handle inner struct only when the target type is 
        /// a value type or the wrapper ValueTypeHolder type.  In addition, the call 
        /// must also be executed in the non-static context since static 
        /// context doesn't need to handle inner struct case.
        /// </summary>
        public bool ShouldHandleInnerStruct
        {
            get { return IsTargetTypeStruct && !IsStatic; }
        }

        public bool IsTargetTypeStruct
        {
            get { return TargetType.IsValueType; }
        }

        public bool HasNoParam
        {
            get { return ParamTypes == Type.EmptyTypes; }
        }

        public bool IsGeneric
        {
            get { return GenericTypes != Type.EmptyTypes; }
        }

        public bool HasRefParam
        {
            get { return ParamTypes.Any( t => t.IsByRef ); }
        }

        /// <summary>
        /// Two <c>CallInfo</c> instances are considered equaled if the following properties
        /// are equaled: <c>TargetType</c>, <c>Flags</c>, <c>IsStatic</c>, <c>MemberTypes</c>, <c>Name</c>,
        /// <c>ParamTypes</c> and <c>GenericTypes</c>.
        /// </summary>
        public override bool Equals( object obj )
        {
            var other = obj as CallInfo;
            if( other == null )
            {
                return false;
            }
            if( other == this )
            {
                return true;
            }

            if( other.TargetType != TargetType ||
                other.Name != Name ||
                other.MemberTypes != MemberTypes ||
                other.BindingFlags != BindingFlags ||
				other.IsReadOperation != IsReadOperation ||
                other.ParamTypes.Length != ParamTypes.Length ||
                other.GenericTypes.Length != GenericTypes.Length)
            {
                return false;
            }

            for( int i = 0; i < ParamTypes.Length; i++ )
            {
                if( ParamTypes[ i ] != other.ParamTypes[ i ] )
                {
                    return false;
                }
            }

            for (int i = 0; i < GenericTypes.Length; i++)
            {
                if (GenericTypes[i] != other.GenericTypes[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
        	int hash = TargetType.GetHashCode() + (int) MemberTypes * Name.GetHashCode() + BindingFlags.GetHashCode() + IsReadOperation.GetHashCode();
        	for( int i = 0; i < ParamTypes.Length; i++ )
        	{
        	    hash += ParamTypes[ i ].GetHashCode() * (i+1);
        	}
        	for (int i = 0; i < GenericTypes.Length; i++)
        	{
        	    hash += GenericTypes[i].GetHashCode() * (i+1);
        	}
        	return hash;
        }
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Emitter\CtorInvocationEmitter.cs
//------------------------------------------------------------------------------
#region License

// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/

#endregion


namespace Fasterflect.Emitter
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

	internal class CtorInvocationEmitter : InvocationEmitter
    {
        public CtorInvocationEmitter(ConstructorInfo ctorInfo, Flags bindingFlags)
            : this(ctorInfo.DeclaringType, bindingFlags, ctorInfo.GetParameters().ToTypeArray(), ctorInfo) { }

        public CtorInvocationEmitter(Type targetType, Flags bindingFlags, Type[] paramTypes)
            : this(targetType, bindingFlags, paramTypes, null) { }

		private CtorInvocationEmitter(Type targetType, Flags flags, Type[] parameterTypes, ConstructorInfo ctorInfo)
            : base(new CallInfo(targetType, null, flags, MemberTypes.Constructor, targetType.Name, parameterTypes, ctorInfo, true))
		{
		}
        
		protected internal override DynamicMethod CreateDynamicMethod()
		{
            return CreateDynamicMethod("ctor", CallInfo.TargetType, Constants.ObjectType, new[] { Constants.ObjectType });
		}

		protected internal override Delegate CreateDelegate()
		{
			if (CallInfo.IsTargetTypeStruct && CallInfo.HasNoParam) // no-arg struct needs special initialization
			{
			    Generator.DeclareLocal( CallInfo.TargetType );      // TargetType tmp
                Generator.ldloca_s(0)                               // &tmp
			             .initobj( CallInfo.TargetType )            // init_obj(&tmp)
			             .ldloc_0.end();                            // load tmp
			}
			else if (CallInfo.TargetType.IsArray)
			{
			    Generator.ldarg_0                                           // load args[] (method arguments)
                         .ldc_i4_0                                          // load 0
                         .ldelem_ref                                        // load args[0] (length)
                         .unbox_any( typeof(int) )                          // unbox stack
                         .newarr( CallInfo.TargetType.GetElementType() );   // new T[args[0]]
			}
			else
			{
                ConstructorInfo ctorInfo = LookupUtils.GetConstructor(CallInfo);
                byte startUsableLocalIndex = 0;
				if (CallInfo.HasRefParam)
				{
                    startUsableLocalIndex = CreateLocalsForByRefParams(0, ctorInfo); // create by_ref_locals from argument array
					Generator.DeclareLocal(CallInfo.TargetType);                     // TargetType tmp;
                }
                
                PushParamsOrLocalsToStack(0);               // push arguments and by_ref_locals
                Generator.newobj(ctorInfo);                 // ctor (<stack>)

				if (CallInfo.HasRefParam)
				{
                    Generator.stloc(startUsableLocalIndex); // tmp = <stack>;
                    AssignByRefParamsToArray(0);            // store by_ref_locals back to argument array
                    Generator.ldloc(startUsableLocalIndex); // tmp
				}
			}
            Generator.boxIfValueType(CallInfo.TargetType)
                     .ret();                                // return (box)<stack>;
			return Method.CreateDelegate(typeof (ConstructorInvoker));
		}
	}
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Emitter\EmitHelper.cs
//------------------------------------------------------------------------------
#region License
// Copyright � 2009 www.bltoolkit.net

// Permission is hereby granted, free of charge, to any person obtaining a copy of 
// this software and associated documentation files (the "Software"), to deal in the 
// Software without restriction, including without limitation the rights to use, 
// copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the
// Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all 
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN
// AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion


namespace Fasterflect.Emitter
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.SymbolStore;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.InteropServices;

	/// <summary>
	/// A wrapper around the <see cref="ILGenerator"/> class.
	/// </summary>
	/// <seealso cref="System.Reflection.Emit.ILGenerator">ILGenerator Class</seealso>
	internal class EmitHelper
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="EmitHelper"/> class
		/// with the specified <see cref="System.Reflection.Emit.ILGenerator"/>.
		/// </summary>
		/// <param name="ilGenerator">The <see cref="System.Reflection.Emit.ILGenerator"/> to use.</param>
		public EmitHelper( ILGenerator ilGenerator )
		{
			if( ilGenerator == null )
			{
				throw new ArgumentNullException( "ilGenerator" );
			}

			_ilGenerator = ilGenerator;
		}

		private readonly ILGenerator _ilGenerator;

		/// <summary>
		/// Gets MSIL generator.
		/// </summary>
		public ILGenerator ILGenerator
		{
			get { return _ilGenerator; }
		}

		/// <summary>
		/// Converts the supplied <see cref="EmitHelper"/> to a <see cref="ILGenerator"/>.
		/// </summary>
		/// <param name="emitHelper">The <see cref="EmitHelper"/>.</param>
		/// <returns>An ILGenerator.</returns>
		public static implicit operator ILGenerator( EmitHelper emitHelper )
		{
			if( emitHelper == null )
			{
				throw new ArgumentNullException( "emitHelper" );
			}

			return emitHelper.ILGenerator;
		}

		#region ILGenerator Methods
		/// <summary>
		/// Begins a catch block.
		/// </summary>
		/// <param name="exceptionType">The Type object that represents the exception.</param>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.BeginCatchBlock(Type)">ILGenerator.BeginCatchBlock Method</seealso>
		public EmitHelper BeginCatchBlock( Type exceptionType )
		{
			_ilGenerator.BeginCatchBlock( exceptionType );
			return this;
		}

		/// <summary>
		/// Begins an exception block for a filtered exception.
		/// </summary>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.BeginExceptFilterBlock">ILGenerator.BeginCatchBlock Method</seealso>
		public EmitHelper BeginExceptFilterBlock()
		{
			_ilGenerator.BeginExceptFilterBlock();
			return this;
		}

		/// <summary>
		/// Begins an exception block for a non-filtered exception.
		/// </summary>
		/// <returns>The label for the end of the block.</returns>
		public Label BeginExceptionBlock()
		{
			return _ilGenerator.BeginExceptionBlock();
		}

		/// <summary>
		/// Begins an exception fault block in the Microsoft intermediate language (MSIL) stream.
		/// </summary>
		/// <returns>Current instance of the <see cref="EmitHelper"/>.</returns>
		public EmitHelper BeginFaultBlock()
		{
			_ilGenerator.BeginFaultBlock();
			return this;
		}

		/// <summary>
		/// Begins a finally block in the Microsoft intermediate language (MSIL) instruction stream.
		/// </summary>
		/// <returns>Current instance of the <see cref="EmitHelper"/>.</returns>
		public EmitHelper BeginFinallyBlock()
		{
			_ilGenerator.BeginFinallyBlock();
			return this;
		}

		/// <summary>
		/// Begins a lexical scope.
		/// </summary>
		/// <returns>Current instance of the <see cref="EmitHelper"/>.</returns>
		public EmitHelper BeginScope()
		{
			_ilGenerator.BeginScope();
			return this;
		}

		/// <summary>
		/// Declares a local variable.
		/// </summary>
		/// <param name="localType">The Type of the local variable.</param>
		/// <returns>The declared local variable.</returns>
		public LocalBuilder DeclareLocal( Type localType )
		{
			return _ilGenerator.DeclareLocal( localType );
		}

		/// <summary>
		/// Declares a local variable, optionally pinning the object referred to by the variable.
		/// </summary>
		/// <param name="localType">The Type of the local variable.</param>
		/// <param name="pinned"><b>true</b> to pin the object in memory; otherwise, <b>false</b>.</param>
		/// <returns>The declared local variable.</returns>
		public LocalBuilder DeclareLocal( Type localType, bool pinned )
		{
			return _ilGenerator.DeclareLocal( localType, pinned );
		}

		/// <summary>
		/// Declares a new label.
		/// </summary>
		/// <returns>Returns a new label that can be used as a token for branching.</returns>
		public Label DefineLabel()
		{
			return _ilGenerator.DefineLabel();
		}

		/// <summary>
		/// Ends an exception block.
		/// </summary>
		/// <returns>Current instance of the <see cref="EmitHelper"/>.</returns>
		public EmitHelper EndExceptionBlock()
		{
			_ilGenerator.EndExceptionBlock();
			return this;
		}

		/// <summary>
		/// Ends a lexical scope.
		/// </summary>
		/// <returns>Current instance of the <see cref="EmitHelper"/>.</returns>
		public EmitHelper EndScope()
		{
			_ilGenerator.EndScope();
			return this;
		}

		/// <summary>
		/// Marks the Microsoft intermediate language (MSIL) stream's current position 
		/// with the given label.
		/// </summary>
		/// <param name="loc">The label for which to set an index.</param>
		/// <returns>Current instance of the <see cref="EmitHelper"/>.</returns>
		public EmitHelper MarkLabel( Label loc )
		{
			_ilGenerator.MarkLabel( loc );
			return this;
		}

		/// <summary>
		/// Marks a sequence point in the Microsoft intermediate language (MSIL) stream.
		/// </summary>
		/// <param name="document">The document for which the sequence point is being defined.</param>
		/// <param name="startLine">The line where the sequence point begins.</param>
		/// <param name="startColumn">The column in the line where the sequence point begins.</param>
		/// <param name="endLine">The line where the sequence point ends.</param>
		/// <param name="endColumn">The column in the line where the sequence point ends.</param>
		/// <returns>Current instance of the <see cref="EmitHelper"/>.</returns>
		public EmitHelper MarkSequencePoint(
			ISymbolDocumentWriter document,
			int startLine,
			int startColumn,
			int endLine,
			int endColumn )
		{
			_ilGenerator.MarkSequencePoint( document, startLine, startColumn, endLine, endColumn );
			return this;
		}

		/// <summary>
		/// Emits an instruction to throw an exception.
		/// </summary>
		/// <param name="exceptionType">The class of the type of exception to throw.</param>
		/// <returns>Current instance of the <see cref="EmitHelper"/>.</returns>
		public EmitHelper ThrowException( Type exceptionType )
		{
			_ilGenerator.ThrowException( exceptionType );
			return this;
		}

		/// <summary>
		/// Specifies the namespace to be used in evaluating locals and watches for 
		/// the current active lexical scope.
		/// </summary>
		/// <param name="namespaceName">The namespace to be used in evaluating locals and watches for the current active lexical scope.</param>
		/// <returns>Current instance of the <see cref="EmitHelper"/>.</returns>
		public EmitHelper UsingNamespace( string namespaceName )
		{
			_ilGenerator.UsingNamespace( namespaceName );
			return this;
		}
		#endregion

		#region Addtional Methods
		public EmitHelper ldelem( Type type )
		{
			_ilGenerator.Emit( OpCodes.Ldelem, type );
			return this;
		}

		public EmitHelper stelem( Type type )
		{
			_ilGenerator.Emit( OpCodes.Stelem, type );
			return this;
		}

		public EmitHelper call( bool isStatic, MethodInfo methodInfo )
		{
			_ilGenerator.Emit( isStatic ? OpCodes.Call : OpCodes.Callvirt, methodInfo );
			return this;
		}

		public EmitHelper ldfld( bool isStatic, FieldInfo fieldInfo )
		{
			_ilGenerator.Emit( isStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, fieldInfo );
			return this;
		}

		public EmitHelper stfld( bool isStatic, FieldInfo fieldInfo )
		{
			_ilGenerator.Emit( isStatic ? OpCodes.Stsfld : OpCodes.Stfld, fieldInfo );
			return this;
		}
		#endregion

		#region Emit Wrappers
		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Add"/>) that
		/// adds two values and pushes the result onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Add">OpCodes.Add</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper add
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Add );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Add_Ovf"/>) that
		/// adds two integers, performs an overflow check, and pushes the result onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Add_Ovf">OpCodes.Add_Ovf</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper add_ovf
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Add_Ovf );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Add_Ovf_Un"/>) that
		/// adds two unsigned integer values, performs an overflow check, and pushes the result onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Add_Ovf_Un">OpCodes.Add_Ovf_Un</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper add_ovf_un
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Add_Ovf_Un );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.And"/>) that
		/// computes the bitwise AND of two values and pushes the result onto the evalution stack.
		/// </summary>
		/// <seealso cref="OpCodes.And">OpCodes.And</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper and
		{
			get
			{
				_ilGenerator.Emit( OpCodes.And );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Arglist"/>) that
		/// returns an unmanaged pointer to the argument list of the current method.
		/// </summary>
		/// <seealso cref="OpCodes.Arglist">OpCodes.Arglist</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper arglist
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Arglist );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Beq"/>, label) that
		/// transfers control to a target instruction if two values are equal.
		/// </summary>
		/// <param name="label">The label to branch from this location.</param>
		/// <seealso cref="OpCodes.Beq">OpCodes.Beq</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper beq( Label label )
		{
			_ilGenerator.Emit( OpCodes.Beq, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Beq_S"/>, label) that
		/// transfers control to a target instruction (short form) if two values are equal.
		/// </summary>
		/// <param name="label">The label to branch from this location.</param>
		/// <seealso cref="OpCodes.Beq_S">OpCodes.Beq_S</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper beq_s( Label label )
		{
			_ilGenerator.Emit( OpCodes.Beq_S, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Bge"/>, label) that
		/// transfers control to a target instruction if the first value is greater than or equal to the second value.
		/// </summary>
		/// <param name="label">The label to branch from this location.</param>
		/// <seealso cref="OpCodes.Bge">OpCodes.Bge</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper bge( Label label )
		{
			_ilGenerator.Emit( OpCodes.Bge, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Bge_S"/>, label) that
		/// transfers control to a target instruction (short form) 
		/// if the first value is greater than or equal to the second value.
		/// </summary>
		/// <param name="label">The label to branch from this location.</param>
		/// <seealso cref="OpCodes.Bge_S">OpCodes.Bge_S</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper bge_s( Label label )
		{
			_ilGenerator.Emit( OpCodes.Bge_S, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Bge_Un"/>, label) that
		/// transfers control to a target instruction if the the first value is greather than the second value,
		/// when comparing unsigned integer values or unordered float values.
		/// </summary>
		/// <param name="label">The label to branch from this location.</param>
		/// <seealso cref="OpCodes.Bge_Un">OpCodes.Bge_Un</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper bge_un( Label label )
		{
			_ilGenerator.Emit( OpCodes.Bge_Un, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Bge_Un_S"/>, label) that
		/// transfers control to a target instruction (short form) if if the the first value is greather than the second value,
		/// when comparing unsigned integer values or unordered float values.
		/// </summary>
		/// <param name="label">The label to branch from this location.</param>
		/// <seealso cref="OpCodes.Bge_Un_S">OpCodes.Bge_Un_S</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper bge_un_s( Label label )
		{
			_ilGenerator.Emit( OpCodes.Bge_Un_S, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Bgt"/>, label) that
		/// transfers control to a target instruction if the first value is greater than the second value.
		/// </summary>
		/// <param name="label">The label to branch from this location.</param>
		/// <seealso cref="OpCodes.Bgt">OpCodes.Bgt</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper bgt( Label label )
		{
			_ilGenerator.Emit( OpCodes.Bgt, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Bgt_S"/>, label) that
		/// transfers control to a target instruction (short form) if the first value is greater than the second value.
		/// </summary>
		/// <param name="label">The label to branch from this location.</param>
		/// <seealso cref="OpCodes.Bgt_S">OpCodes.Bgt_S</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper bgt_s( Label label )
		{
			_ilGenerator.Emit( OpCodes.Bgt_S, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Bgt_Un"/>, label) that
		/// transfers control to a target instruction if the first value is greater than the second value,
		/// when comparing unsigned integer values or unordered float values.
		/// </summary>
		/// <param name="label">The label to branch from this location.</param>
		/// <seealso cref="OpCodes.Bgt_Un">OpCodes.Bgt_Un</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper bgt_un( Label label )
		{
			_ilGenerator.Emit( OpCodes.Bgt_Un, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Bgt_Un_S"/>, label) that
		/// transfers control to a target instruction (short form) if the first value is greater than the second value,
		/// when comparing unsigned integer values or unordered float values.
		/// </summary>
		/// <param name="label">The label to branch from this location.</param>
		/// <seealso cref="OpCodes.Bgt_Un_S">OpCodes.Bgt_Un_S</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper bgt_un_s( Label label )
		{
			_ilGenerator.Emit( OpCodes.Bgt_Un_S, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ble"/>, label) that
		/// transfers control to a target instruction if the first value is less than or equal to the second value.
		/// </summary>
		/// <param name="label">The label to branch from this location.</param>
		/// <seealso cref="OpCodes.Ble">OpCodes.Ble</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper ble( Label label )
		{
			_ilGenerator.Emit( OpCodes.Ble, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ble_S"/>, label) that
		/// transfers control to a target instruction (short form) if the first value is less than or equal to the second value.
		/// </summary>
		/// <param name="label">The label to branch from this location.</param>
		/// <seealso cref="OpCodes.Ble_S">OpCodes.Ble_S</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper ble_s( Label label )
		{
			_ilGenerator.Emit( OpCodes.Ble_S, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ble_Un"/>, label) that
		/// transfers control to a target instruction if the first value is less than or equal to the second value,
		/// when comparing unsigned integer values or unordered float values.
		/// </summary>
		/// <param name="label">The label to branch from this location.</param>
		/// <seealso cref="OpCodes.Ble_Un">OpCodes.Ble_Un</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper ble_un( Label label )
		{
			_ilGenerator.Emit( OpCodes.Ble_Un, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ble_Un_S"/>, label) that
		/// transfers control to a target instruction (short form) if the first value is less than or equal to the second value,
		/// when comparing unsigned integer values or unordered float values.
		/// </summary>
		/// <param name="label">The label to branch from this location.</param>
		/// <seealso cref="OpCodes.Ble_Un_S">OpCodes.Ble_Un_S</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper ble_un_s( Label label )
		{
			_ilGenerator.Emit( OpCodes.Ble_Un_S, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Blt"/>, label) that
		/// transfers control to a target instruction if the first value is less than the second value.
		/// </summary>
		/// <param name="label">The label to branch from this location.</param>
		/// <seealso cref="OpCodes.Blt">OpCodes.Blt</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper blt( Label label )
		{
			_ilGenerator.Emit( OpCodes.Blt, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Blt_S"/>, label) that
		/// transfers control to a target instruction (short form) if the first value is less than the second value.
		/// </summary>
		/// <param name="label">The label to branch from this location.</param>
		/// <seealso cref="OpCodes.Blt_S">OpCodes.Blt_S</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper blt_s( Label label )
		{
			_ilGenerator.Emit( OpCodes.Blt_S, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Blt_Un"/>, label) that
		/// transfers control to a target instruction if the first value is less than the second value,
		/// when comparing unsigned integer values or unordered float values.
		/// </summary>
		/// <param name="label">The label to branch from this location.</param>
		/// <seealso cref="OpCodes.Blt_Un">OpCodes.Blt_Un</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper blt_un( Label label )
		{
			_ilGenerator.Emit( OpCodes.Blt_Un, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Blt_Un_S"/>, label) that
		/// transfers control to a target instruction (short form) if the first value is less than the second value,
		/// when comparing unsigned integer values or unordered float values.
		/// </summary>
		/// <param name="label">The label to branch from this location.</param>
		/// <seealso cref="OpCodes.Blt_Un_S">OpCodes.Blt_Un_S</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper blt_un_s( Label label )
		{
			_ilGenerator.Emit( OpCodes.Blt_Un_S, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Bne_Un"/>, label) that
		/// transfers control to a target instruction when two unsigned integer values or unordered float values are not equal.
		/// </summary>
		/// <param name="label">The label to branch from this location.</param>
		/// <seealso cref="OpCodes.Bne_Un">OpCodes.Bne_Un</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper bne_un( Label label )
		{
			_ilGenerator.Emit( OpCodes.Bne_Un, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Bne_Un_S"/>, label) that
		/// transfers control to a target instruction (short form) 
		/// when two unsigned integer values or unordered float values are not equal.
		/// </summary>
		/// <param name="label">The label to branch from this location.</param>
		/// <seealso cref="OpCodes.Bne_Un_S">OpCodes.Bne_Un_S</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper bne_un_s( Label label )
		{
			_ilGenerator.Emit( OpCodes.Bne_Un_S, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Box"/>, type) that
		/// converts a value type to an object reference.
		/// </summary>
		/// <param name="type">A Type</param>
		/// <seealso cref="OpCodes.Box">OpCodes.Box</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Type)">ILGenerator.Emit</seealso>
		public EmitHelper box( Type type )
		{
			_ilGenerator.Emit( OpCodes.Box, type );
			return this;
		}

		/// <summary>
		/// Converts a value type to an object reference if the value is a value type.
		/// </summary>
		/// <param name="type">A Type</param>
		/// <seealso cref="OpCodes.Box">OpCodes.Box</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Type)">ILGenerator.Emit</seealso>
		public EmitHelper boxIfValueType( Type type )
		{
			if( type == null )
			{
				throw new ArgumentNullException( "type" );
			}

			return type.IsValueType ? box( type ) : this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Br"/>, label) that
		/// unconditionally transfers control to a target instruction. 
		/// </summary>
		/// <param name="label">The label to branch from this location.</param>
		/// <seealso cref="OpCodes.Br">OpCodes.Br</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper br( Label label )
		{
			_ilGenerator.Emit( OpCodes.Br, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Break"/>) that
		/// signals the Common Language Infrastructure (CLI) to inform the debugger that a break point has been tripped.
		/// </summary>
		/// <seealso cref="OpCodes.Break">OpCodes.Break</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper @break
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Break );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Brfalse"/>, label) that
		/// transfers control to a target instruction if value is false, a null reference (Nothing in Visual Basic), or zero.
		/// </summary>
		/// <param name="label">The label to branch from this location.</param>
		/// <seealso cref="OpCodes.Brfalse">OpCodes.Brfalse</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper brfalse( Label label )
		{
			_ilGenerator.Emit( OpCodes.Brfalse, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Brfalse_S"/>, label) that
		/// transfers control to a target instruction if value is false, a null reference, or zero. 
		/// </summary>
		/// <param name="label">The label to branch from this location.</param>
		/// <seealso cref="OpCodes.Brfalse_S">OpCodes.Brfalse_S</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper brfalse_s( Label label )
		{
			_ilGenerator.Emit( OpCodes.Brfalse_S, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Brtrue"/>, label) that
		/// transfers control to a target instruction if value is true, not null, or non-zero.
		/// </summary>
		/// <param name="label">The label to branch from this location.</param>
		/// <seealso cref="OpCodes.Brtrue">OpCodes.Brtrue</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper brtrue( Label label )
		{
			_ilGenerator.Emit( OpCodes.Brtrue, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Brtrue_S"/>, label) that
		/// transfers control to a target instruction (short form) if value is true, not null, or non-zero.
		/// </summary>
		/// <param name="label">The label to branch from this location.</param>
		/// <seealso cref="OpCodes.Brtrue_S">OpCodes.Brtrue_S</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper brtrue_s( Label label )
		{
			_ilGenerator.Emit( OpCodes.Brtrue_S, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Br_S"/>, label) that
		/// unconditionally transfers control to a target instruction (short form).
		/// </summary>
		/// <param name="label">The label to branch from this location.</param>
		/// <seealso cref="OpCodes.Br_S">OpCodes.Br_S</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper br_s( Label label )
		{
			_ilGenerator.Emit( OpCodes.Br_S, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Call"/>, methodInfo) that
		/// calls the method indicated by the passed method descriptor.
		/// </summary>
		/// <param name="methodInfo">The method to be called.</param>
		/// <seealso cref="OpCodes.Call">OpCodes.Call</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,MethodInfo)">ILGenerator.Emit</seealso>
		public EmitHelper call( MethodInfo methodInfo )
		{
			_ilGenerator.Emit( OpCodes.Call, methodInfo );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Call"/>, constructorInfo) that
		/// calls the method indicated by the passed method descriptor.
		/// </summary>
		/// <param name="constructorInfo">The constructor to be called.</param>
		/// <seealso cref="OpCodes.Call">OpCodes.Call</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,MethodInfo)">ILGenerator.Emit</seealso>
		public EmitHelper call( ConstructorInfo constructorInfo )
		{
			_ilGenerator.Emit( OpCodes.Call, constructorInfo );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.EmitCall(<see cref="OpCodes.Call"/>, methodInfo, optionalParameterTypes) that
		/// calls the method indicated by the passed method descriptor.
		/// </summary>
		/// <param name="methodInfo">The method to be called.</param>
		/// <param name="optionalParameterTypes">The types of the optional arguments if the method is a varargs method.</param>
		/// <seealso cref="OpCodes.Call">OpCodes.Call</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.EmitCall(OpCode,MethodInfo,Type[])">ILGenerator.EmitCall</seealso>
		public EmitHelper call( MethodInfo methodInfo, Type[] optionalParameterTypes )
		{
			_ilGenerator.EmitCall( OpCodes.Call, methodInfo, optionalParameterTypes );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.EmitCall(<see cref="OpCodes.Call"/>, methodInfo, optionalParameterTypes) that
		/// calls the method indicated by the passed method descriptor.
		/// </summary>
		/// <param name="type">A Type</param>
		/// <param name="methodName">The name of the method to be called.</param>
		/// <param name="optionalParameterTypes">The types of the optional arguments if the method is a varargs method.</param>
		/// <seealso cref="OpCodes.Call">OpCodes.Call</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.EmitCall(OpCode,MethodInfo,Type[])">ILGenerator.EmitCall</seealso>
		public EmitHelper call( Type type, string methodName, params Type[] optionalParameterTypes )
		{
			if( type == null )
			{
				throw new ArgumentNullException( "type" );
			}

			MethodInfo methodInfo = type.GetMethod( methodName, optionalParameterTypes );

			if( methodInfo == null )
			{
				throw CreateNoSuchMethodException( type, methodName );
			}

			return call( methodInfo );
		}

		/// <summary>
		/// Calls ILGenerator.EmitCall(<see cref="OpCodes.Call"/>, methodInfo, optionalParameterTypes) that
		/// calls the method indicated by the passed method descriptor.
		/// </summary>
		/// <param name="type">A Type</param>
		/// <param name="methodName">The name of the method to be called.</param>
		/// <param name="bindingFlags">A bitmask comprised of one or more <see cref="BindingFlags"/> 
		/// that specify how the search is conducted.</param>
		/// <param name="optionalParameterTypes">The types of the optional arguments if the method is a varargs method.</param>
		/// <seealso cref="OpCodes.Call">OpCodes.Call</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.EmitCall(OpCode,MethodInfo,Type[])">ILGenerator.EmitCall</seealso>
		public EmitHelper call( Type type, string methodName, BindingFlags bindingFlags,
		                        params Type[] optionalParameterTypes )
		{
			if( type == null )
			{
				throw new ArgumentNullException( "type" );
			}

			MethodInfo methodInfo = type.GetMethod( methodName, bindingFlags, null, optionalParameterTypes, null );

			if( methodInfo == null )
			{
				throw CreateNoSuchMethodException( type, methodName );
			}

			return call( methodInfo );
		}

		/// <summary>
		/// Calls ILGenerator.EmitCalli(<see cref="OpCodes.Calli"/>, <see cref="CallingConvention"/>, Type, Type[]) that
		/// calls the method indicated on the evaluation stack (as a pointer to an entry point) 
		/// with arguments described by a calling convention using an unmanaged calling convention.
		/// </summary>
		/// <param name="unmanagedCallConv">The unmanaged calling convention to be used.</param>
		/// <param name="returnType">The Type of the result.</param>
		/// <param name="parameterTypes">The types of the required arguments to the instruction.</param>
		/// <seealso cref="OpCodes.Calli">OpCodes.Calli</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.EmitCalli(OpCode,CallingConvention,Type,Type[])">ILGenerator.EmitCalli</seealso>
		public EmitHelper calli( CallingConvention unmanagedCallConv, Type returnType, Type[] parameterTypes )
		{
			_ilGenerator.EmitCalli( OpCodes.Calli, unmanagedCallConv, returnType, parameterTypes );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.EmitCalli(<see cref="OpCodes.Calli"/>, <see cref="CallingConvention"/>, Type, Type[], Type[]) that
		/// calls the method indicated on the evaluation stack (as a pointer to an entry point)
		/// with arguments described by a calling convention using a managed calling convention.
		/// </summary>
		/// <param name="callingConvention">The managed calling convention to be used.</param>
		/// <param name="returnType">The Type of the result.</param>
		/// <param name="parameterTypes">The types of the required arguments to the instruction.</param>
		/// <param name="optionalParameterTypes">The types of the optional arguments for vararg calls.</param>
		/// <seealso cref="OpCodes.Calli">OpCodes.Calli</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.EmitCalli(OpCode,CallingConventions,Type,Type[],Type[])">ILGenerator.EmitCalli</seealso>
		public EmitHelper calli( CallingConventions callingConvention, Type returnType, Type[] parameterTypes,
		                         Type[] optionalParameterTypes )
		{
			_ilGenerator.EmitCalli( OpCodes.Calli, callingConvention, returnType, parameterTypes, optionalParameterTypes );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Callvirt"/>, methodInfo) that
		/// calls a late-bound method on an object, pushing the return value onto the evaluation stack.
		/// </summary>
		/// <param name="methodInfo">The method to be called.</param>
		/// <seealso cref="OpCodes.Callvirt">OpCodes.Callvirt</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,MethodInfo)">ILGenerator.Emit</seealso>
		public EmitHelper callvirt( MethodInfo methodInfo )
		{
			_ilGenerator.Emit( OpCodes.Callvirt, methodInfo );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.EmitCall(<see cref="OpCodes.Callvirt"/>, methodInfo, optionalParameterTypes) that
		/// calls a late-bound method on an object, pushing the return value onto the evaluation stack.
		/// </summary>
		/// <param name="methodInfo">The method to be called.</param>
		/// <param name="optionalParameterTypes">The types of the optional arguments if the method is a varargs method.</param>
		/// <seealso cref="OpCodes.Callvirt">OpCodes.Callvirt</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.EmitCall(OpCode,MethodInfo,Type[])">ILGenerator.EmitCall</seealso>
		public EmitHelper callvirt( MethodInfo methodInfo, Type[] optionalParameterTypes )
		{
			_ilGenerator.EmitCall( OpCodes.Callvirt, methodInfo, optionalParameterTypes );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.EmitCall(<see cref="OpCodes.Callvirt"/>, methodInfo, optionalParameterTypes) that
		/// calls a late-bound method on an object, pushing the return value onto the evaluation stack.
		/// </summary>
		/// <param name="methodName">The method to be called.</param>
		/// <param name="type">The declaring type of the method.</param>
		/// <param name="optionalParameterTypes">The types of the optional arguments if the method is a varargs method.</param>
		/// <seealso cref="OpCodes.Callvirt">OpCodes.Callvirt</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.EmitCall(OpCode,MethodInfo,Type[])">ILGenerator.EmitCall</seealso>
		public EmitHelper callvirt( Type type, string methodName, params Type[] optionalParameterTypes )
		{
			if( type == null )
			{
				throw new ArgumentNullException( "type" );
			}

			MethodInfo methodInfo = type.GetMethod( methodName, optionalParameterTypes );

			if( methodInfo == null )
			{
				throw CreateNoSuchMethodException( type, methodName );
			}

			return callvirt( methodInfo );
		}

		/// <summary>
		/// Calls ILGenerator.EmitCall(<see cref="OpCodes.Callvirt"/>, methodInfo, optionalParameterTypes) that
		/// calls a late-bound method on an object, pushing the return value onto the evaluation stack.
		/// </summary>
		/// <param name="methodName">The method to be called.</param>
		/// <param name="type">The declaring type of the method.</param>
		/// <param name="bindingFlags">A bitmask comprised of one or more <see cref="BindingFlags"/> 
		/// that specify how the search is conducted.</param>
		/// <param name="optionalParameterTypes">The types of the optional arguments if the method is a varargs method.</param>
		/// <seealso cref="OpCodes.Callvirt">OpCodes.Callvirt</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.EmitCall(OpCode,MethodInfo,Type[])">ILGenerator.EmitCall</seealso>
		public EmitHelper callvirt( Type type, string methodName, BindingFlags bindingFlags,
		                            params Type[] optionalParameterTypes )
		{
			MethodInfo methodInfo =
				optionalParameterTypes == null
					? type.GetMethod( methodName, bindingFlags )
					: type.GetMethod( methodName, bindingFlags, null, optionalParameterTypes, null );

			if( methodInfo == null )
			{
				throw CreateNoSuchMethodException( type, methodName );
			}

			return callvirt( methodInfo, null );
		}

		/// <summary>
		/// Calls ILGenerator.EmitCall(<see cref="OpCodes.Callvirt"/>, methodInfo, optionalParameterTypes) that
		/// calls a late-bound method on an object, pushing the return value onto the evaluation stack.
		/// </summary>
		/// <param name="methodName">The method to be called.</param>
		/// <param name="type">The declaring type of the method.</param>
		/// <param name="bindingFlags">A bitmask comprised of one or more <see cref="BindingFlags"/> 
		/// that specify how the search is conducted.</param>
		/// <seealso cref="OpCodes.Callvirt">OpCodes.Callvirt</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.EmitCall(OpCode,MethodInfo,Type[])">ILGenerator.EmitCall</seealso>
		public EmitHelper callvirt( Type type, string methodName, BindingFlags bindingFlags )
		{
			return callvirt( type, methodName, bindingFlags, null );
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Castclass"/>, type) that
		/// attempts to cast an object passed by reference to the specified class.
		/// </summary>
		/// <param name="type">A Type</param>
		/// <seealso cref="OpCodes.Castclass">OpCodes.Castclass</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Type)">ILGenerator.Emit</seealso>
		public EmitHelper castclass( Type type )
		{
			_ilGenerator.Emit( OpCodes.Castclass, type );
			return this;
		}

		/// <summary>
		/// Attempts to cast an object passed by reference to the specified class 
		/// or to unbox if the type is a value type.
		/// </summary>
		/// <param name="type">A Type</param>
		public EmitHelper castType( Type type )
		{
			if( type == null )
			{
				throw new ArgumentNullException( "type" );
			}

			return type.IsValueType ? unbox_any( type ) : castclass( type );
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ceq"/>) that
		/// compares two values. If they are equal, the integer value 1 (int32) is pushed onto the evaluation stack;
		/// otherwise 0 (int32) is pushed onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Ceq">OpCodes.Ceq</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ceq
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ceq );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Cgt"/>) that
		/// compares two values. If the first value is greater than the second,
		/// the integer value 1 (int32) is pushed onto the evaluation stack;
		/// otherwise 0 (int32) is pushed onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Cgt">OpCodes.Cgt</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper cgt
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Cgt );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Cgt_Un"/>) that
		/// compares two unsigned or unordered values.
		/// If the first value is greater than the second, the integer value 1 (int32) is pushed onto the evaluation stack;
		/// otherwise 0 (int32) is pushed onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Cgt_Un">OpCodes.Cgt_Un</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper cgt_un
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Cgt_Un );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Constrained"/>) that
		/// constrains the type on which a virtual method call is made.
		/// </summary>
		/// <param name="type">A Type</param>
		/// <seealso cref="OpCodes.Cgt_Un">OpCodes.Constrained</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper constrained( Type type )
		{
			_ilGenerator.Emit( OpCodes.Constrained, type );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ckfinite"/>) that
		/// throws <see cref="ArithmeticException"/> if value is not a finite number.
		/// </summary>
		/// <seealso cref="OpCodes.Ckfinite">OpCodes.Ckfinite</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ckfinite
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ckfinite );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Clt"/>) that
		/// compares two values. If the first value is less than the second,
		/// the integer value 1 (int32) is pushed onto the evaluation stack;
		/// otherwise 0 (int32) is pushed onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Clt">OpCodes.Clt</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper clt
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Clt );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Clt_Un"/>) that
		/// compares the unsigned or unordered values value1 and value2.
		/// If value1 is less than value2, then the integer value 1 (int32) is pushed onto the evaluation stack;
		/// otherwise 0 (int32) is pushed onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Clt_Un">OpCodes.Clt_Un</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper clt_un
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Clt_Un );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_I"/>) that
		/// converts the value on top of the evaluation stack to natural int.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_I">OpCodes.Conv_I</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_i
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_I );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_I1"/>) that
		/// converts the value on top of the evaluation stack to int8, then extends (pads) it to int32.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_I1">OpCodes.Conv_I1</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_i1
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_I1 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_I2"/>) that
		/// converts the value on top of the evaluation stack to int16, then extends (pads) it to int32.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_I2">OpCodes.Conv_I2</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_i2
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_I2 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_I4"/>) that
		/// converts the value on top of the evaluation stack to int32.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_I4">OpCodes.Conv_I4</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_i4
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_I4 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_I8"/>) that
		/// converts the value on top of the evaluation stack to int64.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_I8">OpCodes.Conv_I8</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_i8
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_I8 );
				return this;
			}
		}

		/// <summary>
		/// Converts the value on top of the evaluation stack to the specified type.
		/// </summary>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv( Type type )
		{
			if( type == null )
			{
				throw new ArgumentNullException( "type" );
			}

			switch( Type.GetTypeCode( type ) )
			{
				case TypeCode.Boolean:
				case TypeCode.SByte:
					conv_i1.end();
					break;
				case TypeCode.Int16:
					conv_i2.end();
					break;
				case TypeCode.Int32:
					conv_i4.end();
					break;
				case TypeCode.Int64:
					conv_i8.end();
					break;

				case TypeCode.Byte:
					conv_u1.end();
					break;
				case TypeCode.Char:
				case TypeCode.UInt16:
					conv_u2.end();
					break;
				case TypeCode.UInt32:
					conv_u4.end();
					break;
				case TypeCode.UInt64:
					conv_u8.end();
					break;

				case TypeCode.Single:
					conv_r4.end();
					break;
				case TypeCode.Double:
					conv_r8.end();
					break;

				default:
				{
					if( type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) )
					{
						ConstructorInfo ci = type.GetConstructor( type.GetGenericArguments() );
						if( ci != null )
						{
							newobj( ci );
							break;
						}
					}

					throw CreateNotExpectedTypeException( type );
				}
			}

			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_Ovf_I"/>) that
		/// converts the signed value on top of the evaluation stack to signed natural int,
		/// throwing <see cref="OverflowException"/> on overflow.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_Ovf_I">OpCodes.Conv_Ovf_I</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_ovf_i
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_Ovf_I );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_Ovf_I1"/>) that
		/// converts the signed value on top of the evaluation stack to signed int8 and extends it to int32,
		/// throwing <see cref="OverflowException"/> on overflow.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_Ovf_I1">OpCodes.Conv_Ovf_I1</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_ovf_i1
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_Ovf_I1 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_Ovf_I1_Un"/>) that
		/// converts the unsigned value on top of the evaluation stack to signed int8 and extends it to int32,
		/// throwing <see cref="OverflowException"/> on overflow.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_Ovf_I1_Un">OpCodes.Conv_Ovf_I1_Un</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_ovf_i1_un
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_Ovf_I1_Un );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_Ovf_I2"/>) that
		/// converts the signed value on top of the evaluation stack to signed int16 and extending it to int32,
		/// throwing <see cref="OverflowException"/> on overflow.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_Ovf_I2">OpCodes.Conv_Ovf_I2</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_ovf_i2
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_Ovf_I2 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_Ovf_I2_Un"/>) that
		/// converts the unsigned value on top of the evaluation stack to signed int16 and extends it to int32,
		/// throwing <see cref="OverflowException"/> on overflow.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_Ovf_I2_Un">OpCodes.Conv_Ovf_I2_Un</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_ovf_i2_un
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_Ovf_I2_Un );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_Ovf_I4"/>) that
		/// converts the signed value on top of the evaluation tack to signed int32, throwing <see cref="OverflowException"/> on overflow.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_Ovf_I4">OpCodes.Conv_Ovf_I4</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_ovf_i4
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_Ovf_I2_Un );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_Ovf_I4_Un"/>) that
		/// converts the unsigned value on top of the evaluation stack to signed int32, throwing <see cref="OverflowException"/> on overflow.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_Ovf_I4_Un">OpCodes.Conv_Ovf_I4_Un</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_ovf_i4_un
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_Ovf_I4_Un );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_Ovf_I8"/>) that
		/// converts the signed value on top of the evaluation stack to signed int64,
		/// throwing <see cref="OverflowException"/> on overflow.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_Ovf_I8">OpCodes.Conv_Ovf_I8</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_ovf_i8
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_Ovf_I8 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_Ovf_I8_Un"/>) that
		/// converts the unsigned value on top of the evaluation stack to signed int64, throwing <see cref="OverflowException"/> on overflow.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_Ovf_I8_Un">OpCodes.Conv_Ovf_I8_Un</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_ovf_i8_un
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_Ovf_I8_Un );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_Ovf_I_Un"/>) that
		/// converts the unsigned value on top of the evaluation stack to signed natural int,
		/// throwing <see cref="OverflowException"/> on overflow.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_Ovf_I_Un">OpCodes.Conv_Ovf_I_Un</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_ovf_i_un
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_Ovf_I_Un );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_Ovf_U"/>) that
		/// converts the signed value on top of the evaluation stack to unsigned natural int,
		/// throwing <see cref="OverflowException"/> on overflow.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_Ovf_U">OpCodes.Conv_Ovf_U</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_ovf_u
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_Ovf_U );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_Ovf_U1"/>) that
		/// converts the signed value on top of the evaluation stack to unsigned int8 and extends it to int32,
		/// throwing <see cref="OverflowException"/> on overflow.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_Ovf_U1">OpCodes.Conv_Ovf_U1</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_ovf_u1
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_Ovf_U1 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_Ovf_U1_Un"/>) that
		/// converts the unsigned value on top of the evaluation stack to unsigned int8 and extends it to int32,
		/// throwing <see cref="OverflowException"/> on overflow.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_Ovf_U1_Un">OpCodes.Conv_Ovf_U1_Un</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_ovf_u1_un
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_Ovf_U1_Un );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_Ovf_U2"/>) that
		/// converts the signed value on top of the evaluation stack to unsigned int16 and extends it to int32,
		/// throwing <see cref="OverflowException"/> on overflow.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_Ovf_U2">OpCodes.Conv_Ovf_U2</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_ovf_u2
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_Ovf_U2 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_Ovf_U2_Un"/>) that
		/// converts the unsigned value on top of the evaluation stack to unsigned int16 and extends it to int32,
		/// throwing <see cref="OverflowException"/> on overflow.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_Ovf_U2_Un">OpCodes.Conv_Ovf_U2_Un</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_ovf_u2_un
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_Ovf_U2_Un );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_Ovf_U4"/>) that
		/// Converts the signed value on top of the evaluation stack to unsigned int32, throwing <see cref="OverflowException"/> on overflow.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_Ovf_U4">OpCodes.Conv_Ovf_U4</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_ovf_u4
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_Ovf_U4 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_Ovf_U4_Un"/>) that
		/// converts the unsigned value on top of the evaluation stack to unsigned int32, throwing <see cref="OverflowException"/> on overflow.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_Ovf_U4_Un">OpCodes.Conv_Ovf_U4_Un</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_ovf_u4_un
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_Ovf_U4_Un );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_Ovf_U8"/>) that
		/// converts the signed value on top of the evaluation stack to unsigned int64, throwing <see cref="OverflowException"/> on overflow.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_Ovf_U8">OpCodes.Conv_Ovf_U8</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_ovf_u8
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_Ovf_U8 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_Ovf_U8_Un"/>) that
		/// converts the unsigned value on top of the evaluation stack to unsigned int64, throwing <see cref="OverflowException"/> on overflow.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_Ovf_U8_Un">OpCodes.Conv_Ovf_U8_Un</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_ovf_u8_un
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_Ovf_U8_Un );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_Ovf_U_Un"/>) that
		/// converts the unsigned value on top of the evaluation stack to unsigned natural int,
		/// throwing <see cref="OverflowException"/> on overflow.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_Ovf_U_Un">OpCodes.Conv_Ovf_U_Un</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_ovf_u_un
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_Ovf_U_Un );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_R4"/>) that
		/// converts the value on top of the evaluation stack to float32.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_R4">OpCodes.Conv_R4</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_r4
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_R4 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_R8"/>) that
		/// converts the value on top of the evaluation stack to float64.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_R8">OpCodes.Conv_R8</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_r8
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_R8 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_R_Un"/>) that
		/// converts the unsigned integer value on top of the evaluation stack to float32.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_R_Un">OpCodes.Conv_R_Un</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_r_un
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_R_Un );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_U"/>) that
		/// converts the value on top of the evaluation stack to unsigned natural int, and extends it to natural int.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_U">OpCodes.Conv_U</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_u
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_U );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_U1"/>) that
		/// converts the value on top of the evaluation stack to unsigned int8, and extends it to int32.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_U1">OpCodes.Conv_U1</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_u1
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_U1 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_U2"/>) that
		/// converts the value on top of the evaluation stack to unsigned int16, and extends it to int32.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_U2">OpCodes.Conv_U2</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_u2
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_U2 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_U4"/>) that
		/// converts the value on top of the evaluation stack to unsigned int32, and extends it to int32.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_U4">OpCodes.Conv_U4</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_u4
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_U4 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Conv_U8"/>) that
		/// converts the value on top of the evaluation stack to unsigned int64, and extends it to int64.
		/// </summary>
		/// <seealso cref="OpCodes.Conv_U8">OpCodes.Conv_U8</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper conv_u8
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Conv_U8 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Cpblk"/>) that
		/// copies a specified number bytes from a source address to a destination address.
		/// </summary>
		/// <seealso cref="OpCodes.Cpblk">OpCodes.Cpblk</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper cpblk
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Cpblk );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Cpobj"/>, type) that
		/// copies the value type located at the address of an object (type &amp;, * or natural int) 
		/// to the address of the destination object (type &amp;, * or natural int).
		/// </summary>
		/// <param name="type">A Type</param>
		/// <seealso cref="OpCodes.Cpobj">OpCodes.Cpobj</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Type)">ILGenerator.Emit</seealso>
		public EmitHelper cpobj( Type type )
		{
			_ilGenerator.Emit( OpCodes.Cpobj, type );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Div"/>) that
		/// divides two values and pushes the result as a floating-point (type F) or
		/// quotient (type int32) onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Div">OpCodes.Div</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper div
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Div );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Div_Un"/>) that
		/// divides two unsigned integer values and pushes the result (int32) onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Div_Un">OpCodes.Div_Un</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper div_un
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Div_Un );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Dup"/>) that
		/// copies the current topmost value on the evaluation stack, and then pushes the copy onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Dup">OpCodes.Dup</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper dup
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Dup );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Endfilter"/>) that
		/// transfers control from the filter clause of an exception back to
		/// the Common Language Infrastructure (CLI) exception handler.
		/// </summary>
		/// <seealso cref="OpCodes.Endfilter">OpCodes.Endfilter</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper endfilter
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Endfilter );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Endfinally"/>) that
		/// transfers control from the fault or finally clause of an exception block back to
		/// the Common Language Infrastructure (CLI) exception handler.
		/// </summary>
		/// <seealso cref="OpCodes.Endfinally">OpCodes.Endfinally</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper endfinally
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Endfinally );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Initblk"/>) that
		/// initializes a specified block of memory at a specific address to a given size and initial value.
		/// </summary>
		/// <seealso cref="OpCodes.Initblk">OpCodes.Initblk</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper initblk
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Initblk );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Initobj"/>, type) that
		/// initializes all the fields of the object at a specific address to a null reference or 
		/// a 0 of the appropriate primitive type.
		/// </summary>
		/// <param name="type">A Type</param>
		/// <seealso cref="OpCodes.Initobj">OpCodes.Initobj</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Type)">ILGenerator.Emit</seealso>
		public EmitHelper initobj( Type type )
		{
			_ilGenerator.Emit( OpCodes.Initobj, type );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Isinst"/>, type) that
		/// tests whether an object reference (type O) is an instance of a particular class.
		/// </summary>
		/// <param name="type">A Type</param>
		/// <seealso cref="OpCodes.Isinst">OpCodes.Isinst</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Type)">ILGenerator.Emit</seealso>
		public EmitHelper isinst( Type type )
		{
			_ilGenerator.Emit( OpCodes.Isinst, type );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Jmp"/>, methodInfo) that
		/// exits current method and jumps to specified method.
		/// </summary>
		/// <param name="methodInfo">The method to be jumped.</param>
		/// <seealso cref="OpCodes.Jmp">OpCodes.Jmp</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,MethodInfo)">ILGenerator.Emit</seealso>
		public EmitHelper jmp( MethodInfo methodInfo )
		{
			_ilGenerator.Emit( OpCodes.Jmp, methodInfo );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldarg"/>, short) that
		/// loads an argument (referenced by a specified index value) onto the stack.
		/// </summary>
		/// <param name="index">Index of the argument that is pushed onto the stack.</param>
		/// <seealso cref="OpCodes.Ldarg">OpCodes.Ldarg</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,short)">ILGenerator.Emit</seealso>
		public EmitHelper ldarg( short index )
		{
			_ilGenerator.Emit( OpCodes.Ldarg, index );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldarg"/>, short) or 
		/// ILGenerator.Emit(<see cref="OpCodes.Ldarg_S"/>, byte) that
		/// loads an argument (referenced by a specified index value) onto the stack.
		/// </summary>
		/// <param name="index">Index of the argument that is pushed onto the stack.</param>
		/// <seealso cref="OpCodes.Ldarg">OpCodes.Ldarg</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,short)">ILGenerator.Emit</seealso>
		public EmitHelper ldarg( int index )
		{
			switch( index )
			{
				case 0:
					ldarg_0.end();
					break;
				case 1:
					ldarg_1.end();
					break;
				case 2:
					ldarg_2.end();
					break;
				case 3:
					ldarg_3.end();
					break;
				default:
					if( index <= byte.MaxValue )
					{
						ldarg_s( (byte) index );
					}
					else if( index <= short.MaxValue )
					{
						ldarg( (short) index );
					}
					else
					{
						throw new ArgumentOutOfRangeException( "index" );
					}

					break;
			}

			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldarga"/>, short) that
		/// load an argument address onto the evaluation stack.
		/// </summary>
		/// <param name="index">Index of the address addr of the argument that is pushed onto the stack.</param>
		/// <seealso cref="OpCodes.Ldarga">OpCodes.Ldarga</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,short)">ILGenerator.Emit</seealso>
		public EmitHelper ldarga( short index )
		{
			_ilGenerator.Emit( OpCodes.Ldarga, index );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldarga_S"/>, byte) that
		/// load an argument address, in short form, onto the evaluation stack.
		/// </summary>
		/// <param name="index">Index of the address addr of the argument that is pushed onto the stack.</param>
		/// <seealso cref="OpCodes.Ldarga_S">OpCodes.Ldarga_S</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,byte)">ILGenerator.Emit</seealso>
		public EmitHelper ldarga_s( byte index )
		{
			_ilGenerator.Emit( OpCodes.Ldarga_S, index );
			return this;
		}

		/// <summary>
		/// Load an argument address onto the evaluation stack.
		/// </summary>
		/// <param name="index">Index of the address addr of the argument that is pushed onto the stack.</param>
		public EmitHelper ldarga( int index )
		{
			if( index <= byte.MaxValue )
			{
				ldarga_s( (byte) index );
			}
			else if( index <= short.MaxValue )
			{
				ldarga( (short) index );
			}
			else
			{
				throw new ArgumentOutOfRangeException( "index" );
			}

			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldarg_0"/>) that
		/// loads the argument at index 0 onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Ldarg_0">OpCodes.Ldarg_0</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldarg_0
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldarg_0 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldarg_1"/>) that
		/// loads the argument at index 1 onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Ldarg_1">OpCodes.Ldarg_1</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldarg_1
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldarg_1 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldarg_2"/>) that
		/// loads the argument at index 2 onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Ldarg_2">OpCodes.Ldarg_2</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldarg_2
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldarg_2 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldarg_3"/>) that
		/// loads the argument at index 3 onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Ldarg_3">OpCodes.Ldarg_3</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldarg_3
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldarg_3 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldarg_S"/>, byte) that
		/// loads the argument (referenced by a specified short form index) onto the evaluation stack.
		/// </summary>
		/// <param name="index">Index of the argument value that is pushed onto the stack.</param>
		/// <seealso cref="OpCodes.Ldarg_S">OpCodes.Ldarg_S</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,byte)">ILGenerator.Emit</seealso>
		public EmitHelper ldarg_s( byte index )
		{
			_ilGenerator.Emit( OpCodes.Ldarg_S, index );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldc_I4_0"/> or <see cref="OpCodes.Ldc_I4_1"/>) that
		/// pushes a supplied value of type int32 onto the evaluation stack as an int32.
		/// </summary>
		/// <param name="b">The value pushed onto the stack.</param>
		/// <seealso cref="OpCodes.Ldc_I4">OpCodes.Ldc_I4_0</seealso>
		/// <seealso cref="OpCodes.Ldc_I4">OpCodes.Ldc_I4_1</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,int)">ILGenerator.Emit</seealso>
		public EmitHelper ldc_bool( bool b )
		{
			_ilGenerator.Emit( b ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0 );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldc_I4"/>, int) that
		/// pushes a supplied value of type int32 onto the evaluation stack as an int32.
		/// </summary>
		/// <param name="num">The value pushed onto the stack.</param>
		/// <seealso cref="OpCodes.Ldc_I4">OpCodes.Ldc_I4</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,int)">ILGenerator.Emit</seealso>
		public EmitHelper ldc_i4( int num )
		{
			_ilGenerator.Emit( OpCodes.Ldc_I4, num );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldc_I4_0"/>) that
		/// pushes the integer value of 0 onto the evaluation stack as an int32.
		/// </summary>
		/// <seealso cref="OpCodes.Ldc_I4_0">OpCodes.Ldc_I4_0</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldc_i4_0
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldc_I4_0 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldc_I4_1"/>) that
		/// pushes the integer value of 1 onto the evaluation stack as an int32.
		/// </summary>
		/// <seealso cref="OpCodes.Ldc_I4_1">OpCodes.Ldc_I4_1</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldc_i4_1
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldc_I4_1 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldc_I4_2"/>) that
		/// pushes the integer value of 2 onto the evaluation stack as an int32.
		/// </summary>
		/// <seealso cref="OpCodes.Ldc_I4_2">OpCodes.Ldc_I4_2</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldc_i4_2
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldc_I4_2 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldc_I4_3"/>) that
		/// pushes the integer value of 3 onto the evaluation stack as an int32.
		/// </summary>
		/// <seealso cref="OpCodes.Ldc_I4_3">OpCodes.Ldc_I4_3</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldc_i4_3
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldc_I4_3 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldc_I4_4"/>) that
		/// pushes the integer value of 4 onto the evaluation stack as an int32.
		/// </summary>
		/// <seealso cref="OpCodes.Ldc_I4_4">OpCodes.Ldc_I4_4</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldc_i4_4
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldc_I4_4 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldc_I4_5"/>) that
		/// pushes the integer value of 5 onto the evaluation stack as an int32.
		/// </summary>
		/// <seealso cref="OpCodes.Ldc_I4_5">OpCodes.Ldc_I4_0</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldc_i4_5
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldc_I4_5 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldc_I4_6"/>) that
		/// pushes the integer value of 6 onto the evaluation stack as an int32.
		/// </summary>
		/// <seealso cref="OpCodes.Ldc_I4_6">OpCodes.Ldc_I4_6</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldc_i4_6
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldc_I4_6 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldc_I4_7"/>) that
		/// pushes the integer value of 7 onto the evaluation stack as an int32.
		/// </summary>
		/// <seealso cref="OpCodes.Ldc_I4_7">OpCodes.Ldc_I4_7</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldc_i4_7
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldc_I4_7 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldc_I4_8"/>) that
		/// pushes the integer value of 8 onto the evaluation stack as an int32.
		/// </summary>
		/// <seealso cref="OpCodes.Ldc_I4_8">OpCodes.Ldc_I4_8</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldc_i4_8
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldc_I4_8 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldc_I4_M1"/>) that
		/// pushes the integer value of -1 onto the evaluation stack as an int32.
		/// </summary>
		/// <seealso cref="OpCodes.Ldc_I4_M1">OpCodes.Ldc_I4_M1</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldc_i4_m1
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldc_I4_M1 );
				return this;
			}
		}

		/// <summary>
		/// Calls the best form of ILGenerator.Emit(Ldc_I4_X) that
		/// pushes the integer value of -1 onto the evaluation stack as an int32.
		/// </summary>
		/// <seealso cref="ldc_i4"/>
		public EmitHelper ldc_i4_( int num )
		{
			switch( num )
			{
				case -1:
					ldc_i4_m1.end();
					break;
				case 0:
					ldc_i4_0.end();
					break;
				case 1:
					ldc_i4_1.end();
					break;
				case 2:
					ldc_i4_2.end();
					break;
				case 3:
					ldc_i4_3.end();
					break;
				case 4:
					ldc_i4_4.end();
					break;
				case 5:
					ldc_i4_5.end();
					break;
				case 6:
					ldc_i4_6.end();
					break;
				case 7:
					ldc_i4_7.end();
					break;
				case 8:
					ldc_i4_8.end();
					break;
				default:
					if( num >= sbyte.MinValue && num <= sbyte.MaxValue )
					{
						ldc_i4_s( (sbyte) num );
					}
					else
					{
						ldc_i4( num );
					}

					break;
			}

			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldc_I4_S"/>, byte) that
		/// pushes the supplied int8 value onto the evaluation stack as an int32, short form.
		/// </summary>
		/// <param name="num">The value pushed onto the stack.</param>
		/// <seealso cref="OpCodes.Ldc_I4_S">OpCodes.Ldc_I4_S</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,byte)">ILGenerator.Emit</seealso>
		public EmitHelper ldc_i4_s( sbyte num )
		{
			_ilGenerator.Emit( OpCodes.Ldc_I4_S, num );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldc_I8"/>, long) that
		/// pushes a supplied value of type int64 onto the evaluation stack as an int64.
		/// </summary>
		/// <param name="num">The value pushed onto the stack.</param>
		/// <seealso cref="OpCodes.Ldc_I8">OpCodes.Ldc_I8</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,long)">ILGenerator.Emit</seealso>
		public EmitHelper ldc_i8( long num )
		{
			_ilGenerator.Emit( OpCodes.Ldc_I8, num );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldc_R4"/>, float) that
		/// pushes a supplied value of type float32 onto the evaluation stack as type F (float).
		/// </summary>
		/// <param name="num">The value pushed onto the stack.</param>
		/// <seealso cref="OpCodes.Ldc_R4">OpCodes.Ldc_R4</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,float)">ILGenerator.Emit</seealso>
		public EmitHelper ldc_r4( float num )
		{
			_ilGenerator.Emit( OpCodes.Ldc_R4, num );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldc_R8"/>, double) that
		/// pushes a supplied value of type float64 onto the evaluation stack as type F (float).
		/// </summary>
		/// <param name="num">The value pushed onto the stack.</param>
		/// <seealso cref="OpCodes.Ldc_R8">OpCodes.Ldc_R8</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,double)">ILGenerator.Emit</seealso>
		public EmitHelper ldc_r8( double num )
		{
			_ilGenerator.Emit( OpCodes.Ldc_R8, num );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldelema"/>, type) that
		/// loads the address of the array element at a specified array index onto the top of the evaluation stack 
		/// as type &amp; (managed pointer).
		/// </summary>
		/// <param name="type">A Type</param>
		/// <seealso cref="OpCodes.Ldelema">OpCodes.Ldelema</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Type)">ILGenerator.Emit</seealso>
		public EmitHelper ldelema( Type type )
		{
			_ilGenerator.Emit( OpCodes.Ldelema, type );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldelem_I"/>) that
		/// loads the element with type natural int at a specified array index onto the top of the evaluation stack 
		/// as a natural int.
		/// </summary>
		/// <seealso cref="OpCodes.Ldelem_I">OpCodes.Ldelem_I</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldelem_i
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldelem_I );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldelem_I1"/>) that
		/// loads the element with type int8 at a specified array index onto the top of the evaluation stack as an int32.
		/// </summary>
		/// <seealso cref="OpCodes.Ldelem_I1">OpCodes.Ldelem_I1</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldelem_i1
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldelem_I1 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldelem_I2"/>) that
		/// loads the element with type int16 at a specified array index onto the top of the evaluation stack as an int32.
		/// </summary>
		/// <seealso cref="OpCodes.Ldelem_I2">OpCodes.Ldelem_I2</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldelem_i2
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldelem_I2 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldelem_I4"/>) that
		/// loads the element with type int32 at a specified array index onto the top of the evaluation stack as an int32.
		/// </summary>
		/// <seealso cref="OpCodes.Ldelem_I4">OpCodes.Ldelem_I4</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldelem_i4
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldelem_I4 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldelem_I8"/>) that
		/// loads the element with type int64 at a specified array index onto the top of the evaluation stack as an int64.
		/// </summary>
		/// <seealso cref="OpCodes.Ldelem_I8">OpCodes.Ldelem_I8</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldelem_i8
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldelem_I8 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldelem_R4"/>) that
		/// loads the element with type float32 at a specified array index onto the top of the evaluation stack as type F (float).
		/// </summary>
		/// <seealso cref="OpCodes.Ldelem_R4">OpCodes.Ldelem_R4</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldelem_r4
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldelem_R4 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldelem_R8"/>) that
		/// loads the element with type float64 at a specified array index onto the top of the evaluation stack as type F (float).
		/// </summary>
		/// <seealso cref="OpCodes.Ldelem_R8">OpCodes.Ldelem_R8</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldelem_r8
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldelem_R8 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldelem_Ref"/>) that
		/// loads the element containing an object reference at a specified array index 
		/// onto the top of the evaluation stack as type O (object reference).
		/// </summary>
		/// <seealso cref="OpCodes.Ldelem_Ref">OpCodes.Ldelem_Ref</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldelem_ref
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldelem_Ref );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldelem_U1"/>) that
		/// loads the element with type unsigned int8 at a specified array index onto the top of the evaluation stack as an int32.
		/// </summary>
		/// <seealso cref="OpCodes.Ldelem_U1">OpCodes.Ldelem_U1</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldelem_u1
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldelem_U1 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldelem_U2"/>) that
		/// loads the element with type unsigned int16 at a specified array index 
		/// onto the top of the evaluation stack as an int32.
		/// </summary>
		/// <seealso cref="OpCodes.Ldelem_U2">OpCodes.Ldelem_U2</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldelem_u2
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldelem_U2 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldelem_U4"/>) that
		/// loads the element with type unsigned int32 at a specified array index 
		/// onto the top of the evaluation stack as an int32.
		/// </summary>
		/// <seealso cref="OpCodes.Ldelem_U4">OpCodes.Ldelem_U4</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldelem_u4
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldelem_U4 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldfld"/>, fieldInfo) that
		/// finds the value of a field in the object whose reference is currently on the evaluation stack.
		/// </summary>
		/// <param name="fieldInfo">A <see cref="FieldInfo"/> representing a field.</param>
		/// <seealso cref="OpCodes.Ldfld">OpCodes.Ldfld</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,FieldInfo)">ILGenerator.Emit</seealso>
		public EmitHelper ldfld( FieldInfo fieldInfo )
		{
			_ilGenerator.Emit( OpCodes.Ldfld, fieldInfo );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldflda"/>, fieldInfo) that
		/// finds the address of a field in the object whose reference is currently on the evaluation stack.
		/// </summary>
		/// <param name="fieldInfo">A <see cref="FieldInfo"/> representing a field.</param>
		/// <seealso cref="OpCodes.Ldflda">OpCodes.Ldflda</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,FieldInfo)">ILGenerator.Emit</seealso>
		public EmitHelper ldflda( FieldInfo fieldInfo )
		{
			_ilGenerator.Emit( OpCodes.Ldflda, fieldInfo );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldftn"/>, methodInfo) that
		/// pushes an unmanaged pointer (type natural int) to the native code implementing a specific method 
		/// onto the evaluation stack.
		/// </summary>
		/// <param name="methodInfo">The method to be called.</param>
		/// <seealso cref="OpCodes.Ldftn">OpCodes.Ldftn</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,MethodInfo)">ILGenerator.Emit</seealso>
		public EmitHelper ldftn( MethodInfo methodInfo )
		{
			_ilGenerator.Emit( OpCodes.Ldftn, methodInfo );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldind_I"/>) that
		/// loads a value of type natural int as a natural int onto the evaluation stack indirectly.
		/// </summary>
		/// <seealso cref="OpCodes.Ldind_I">OpCodes.Ldind_I</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldind_i
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldind_I );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldind_I1"/>) that
		/// loads a value of type int8 as an int32 onto the evaluation stack indirectly.
		/// </summary>
		/// <seealso cref="OpCodes.Ldind_I1">OpCodes.Ldind_I1</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldind_i1
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldind_I1 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldind_I2"/>) that
		/// loads a value of type int16 as an int32 onto the evaluation stack indirectly.
		/// </summary>
		/// <seealso cref="OpCodes.Ldind_I2">OpCodes.Ldind_I2</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldind_i2
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldind_I2 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldind_I4"/>) that
		/// loads a value of type int32 as an int32 onto the evaluation stack indirectly.
		/// </summary>
		/// <seealso cref="OpCodes.Ldind_I4">OpCodes.Ldind_I4</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldind_i4
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldind_I4 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldind_I8"/>) that
		/// loads a value of type int64 as an int64 onto the evaluation stack indirectly.
		/// </summary>
		/// <seealso cref="OpCodes.Ldind_I8">OpCodes.Ldind_I8</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldind_i8
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldind_I8 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldind_R4"/>) that
		/// loads a value of type float32 as a type F (float) onto the evaluation stack indirectly.
		/// </summary>
		/// <seealso cref="OpCodes.Ldind_R4">OpCodes.Ldind_R4</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldind_r4
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldind_R4 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldind_R8"/>) that
		/// loads a value of type float64 as a type F (float) onto the evaluation stack indirectly.
		/// </summary>
		/// <seealso cref="OpCodes.Ldind_R8">OpCodes.Ldind_R8</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldind_r8
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldind_R8 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldind_Ref"/>) that
		/// loads an object reference as a type O (object reference) onto the evaluation stack indirectly.
		/// </summary>
		/// <seealso cref="OpCodes.Ldind_Ref">OpCodes.Ldind_Ref</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldind_ref
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldind_Ref );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldind_U1"/>) that
		/// loads a value of type unsigned int8 as an int32 onto the evaluation stack indirectly.
		/// </summary>
		/// <seealso cref="OpCodes.Ldind_U1">OpCodes.Ldind_U1</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldind_u1
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldind_U1 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldind_U2"/>) that
		/// loads a value of type unsigned int16 as an int32 onto the evaluation stack indirectly.
		/// </summary>
		/// <seealso cref="OpCodes.Ldind_U2">OpCodes.Ldind_U2</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldind_u2
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldind_U2 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldind_U4"/>) that
		/// loads a value of type unsigned int32 as an int32 onto the evaluation stack indirectly.
		/// </summary>
		/// <seealso cref="OpCodes.Ldind_U4">OpCodes.Ldind_U4</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldind_u4
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldind_U4 );
				return this;
			}
		}

		/// <summary>
		/// Loads a value of the type from a supplied address.
		/// </summary>
		/// <param name="type">A Type.</param>
		public EmitHelper ldind( Type type )
		{
			if( type == null )
			{
				throw new ArgumentNullException( "type" );
			}

			switch( Type.GetTypeCode( type ) )
			{
				case TypeCode.Boolean:
				case TypeCode.Byte:
				case TypeCode.SByte:
					ldind_i1.end();
					break;

				case TypeCode.Char:
				case TypeCode.Int16:
				case TypeCode.UInt16:
					ldind_i2.end();
					break;

				case TypeCode.Int32:
				case TypeCode.UInt32:
					ldind_i4.end();
					break;

				case TypeCode.Int64:
				case TypeCode.UInt64:
					ldind_i8.end();
					break;

				case TypeCode.Single:
					ldind_r4.end();
					break;
				case TypeCode.Double:
					ldind_r8.end();
					break;

				default:
					if( type.IsClass )
					{
						ldind_ref.end();
					}
					else if( type.IsValueType )
					{
						stobj( type );
					}
					else
					{
						throw CreateNotExpectedTypeException( type );
					}
					break;
			}

			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldlen"/>) that
		/// pushes the number of elements of a zero-based, one-dimensional array onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Ldlen">OpCodes.Ldlen</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldlen
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldlen );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldloc"/>, short) that
		/// load an argument address onto the evaluation stack.
		/// </summary>
		/// <param name="index">Index of the local variable value pushed onto the stack.</param>
		/// <seealso cref="OpCodes.Ldloc">OpCodes.Ldloc</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,short)">ILGenerator.Emit</seealso>
		public EmitHelper ldloc( short index )
		{
			_ilGenerator.Emit( OpCodes.Ldloc, index );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldloc"/>, <see cref="LocalBuilder"/>) that
		/// load an argument address onto the evaluation stack.
		/// </summary>
		/// <param name="localBuilder">Local variable builder.</param>
		/// <seealso cref="OpCodes.Ldloc">OpCodes.Ldloc</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,short)">ILGenerator.Emit</seealso>
		public EmitHelper ldloc( LocalBuilder localBuilder )
		{
			_ilGenerator.Emit( OpCodes.Ldloc, localBuilder );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldloca"/>, short) that
		/// loads the address of the local variable at a specific index onto the evaluation stack.
		/// </summary>
		/// <param name="index">Index of the local variable.</param>
		/// <seealso cref="OpCodes.Ldloca">OpCodes.Ldloca</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,short)">ILGenerator.Emit</seealso>
		public EmitHelper ldloca( short index )
		{
			_ilGenerator.Emit( OpCodes.Ldloca, index );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldloca_S"/>, byte) that
		/// loads the address of the local variable at a specific index onto the evaluation stack, short form.
		/// </summary>
		/// <param name="index">Index of the local variable.</param>
		/// <seealso cref="OpCodes.Ldloca_S">OpCodes.Ldloca_S</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,byte)">ILGenerator.Emit</seealso>
		public EmitHelper ldloca_s( byte index )
		{
			_ilGenerator.Emit( OpCodes.Ldloca_S, index );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldloca"/>, <see cref="LocalBuilder"/>) that
		/// loads the address of the local variable at a specific index onto the evaluation stack.
		/// </summary>
		/// <param name="local">A <see cref="LocalBuilder"/> representing the local variable.</param>
		/// <seealso cref="OpCodes.Ldloca">OpCodes.Ldloca</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,short)">ILGenerator.Emit</seealso>
		public EmitHelper ldloca( LocalBuilder local )
		{
			_ilGenerator.Emit( OpCodes.Ldloca, local );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldloc_0"/>) that
		/// loads the local variable at index 0 onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Ldloc_0">OpCodes.Ldloc_0</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldloc_0
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldloc_0 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldloc_1"/>) that
		/// loads the local variable at index 1 onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Ldloc_1">OpCodes.Ldloc_1</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldloc_1
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldloc_1 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldloc_2"/>) that
		/// loads the local variable at index 2 onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Ldloc_2">OpCodes.Ldloc_2</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldloc_2
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldloc_2 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldloc_3"/>) that
		/// loads the local variable at index 3 onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Ldloc_3">OpCodes.Ldloc_3</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldloc_3
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldloc_3 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldloc_S"/>, byte) that
		/// loads the local variable at a specific index onto the evaluation stack, short form.
		/// </summary>
		/// <param name="index">Index of the local variable.</param>
		/// <seealso cref="OpCodes.Ldloc_S">OpCodes.Ldloc_S</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,byte)">ILGenerator.Emit</seealso>
		public EmitHelper ldloc_s( byte index )
		{
			_ilGenerator.Emit( OpCodes.Ldloca_S, index );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldnull"/>) that
		/// pushes a null reference (type O) onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Ldnull">OpCodes.Ldnull</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ldnull
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Ldnull );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldobj"/>, type) that
		/// copies the value type object pointed to by an address to the top of the evaluation stack.
		/// </summary>
		/// <param name="type">A Type</param>
		/// <seealso cref="OpCodes.Ldobj">OpCodes.Ldobj</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Type)">ILGenerator.Emit</seealso>
		public EmitHelper ldobj( Type type )
		{
			_ilGenerator.Emit( OpCodes.Ldobj, type );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldsfld"/>, fieldInfo) that
		/// pushes the value of a static field onto the evaluation stack.
		/// </summary>
		/// <param name="fieldInfo">A <see cref="FieldInfo"/> representing a field.</param>
		/// <seealso cref="OpCodes.Ldsfld">OpCodes.Ldsfld</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,FieldInfo)">ILGenerator.Emit</seealso>
		public EmitHelper ldsfld( FieldInfo fieldInfo )
		{
			_ilGenerator.Emit( OpCodes.Ldsfld, fieldInfo );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldsflda"/>, fieldInfo) that
		/// pushes the address of a static field onto the evaluation stack.
		/// </summary>
		/// <param name="fieldInfo">A <see cref="FieldInfo"/> representing a field.</param>
		/// <seealso cref="OpCodes.Ldsflda">OpCodes.Ldsflda</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,FieldInfo)">ILGenerator.Emit</seealso>
		public EmitHelper ldsflda( FieldInfo fieldInfo )
		{
			_ilGenerator.Emit( OpCodes.Ldsflda, fieldInfo );
			return this;
		}

		/// <summary>
		/// Calls <see cref="ldstr"/> -or- <see cref="ldnull"/>,
		/// if given string is a null reference.
		/// </summary>
		/// <param name="str">The String to be emitted.</param>
		/// <seealso cref="ldstr"/>
		/// <seealso cref="ldnull"/>
		public EmitHelper ldstrEx( string str )
		{
			return str == null ? ldnull : ldstr( str );
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldstr"/>, string) that
		/// pushes a new object reference to a string literal stored in the metadata.
		/// </summary>
		/// <param name="str">The String to be emitted.</param>
		/// <seealso cref="OpCodes.Ldstr">OpCodes.Ldstr</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,FieldInfo)">ILGenerator.Emit</seealso>
		public EmitHelper ldstr( string str )
		{
			_ilGenerator.Emit( OpCodes.Ldstr, str );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldtoken"/>, methodInfo) that
		/// converts a metadata token to its runtime representation, pushing it onto the evaluation stack.
		/// </summary>
		/// <param name="methodInfo">The method to be called.</param>
		/// <seealso cref="OpCodes.Ldtoken">OpCodes.Ldtoken</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,MethodInfo)">ILGenerator.Emit</seealso>
		public EmitHelper ldtoken( MethodInfo methodInfo )
		{
			_ilGenerator.Emit( OpCodes.Ldtoken, methodInfo );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldtoken"/>, fieldInfo) that
		/// converts a metadata token to its runtime representation, 
		/// pushing it onto the evaluation stack.
		/// </summary>
		/// <param name="fieldInfo">A <see cref="FieldInfo"/> representing a field.</param>
		/// <seealso cref="OpCodes.Ldtoken">OpCodes.Ldtoken</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,FieldInfo)">ILGenerator.Emit</seealso>
		public EmitHelper ldtoken( FieldInfo fieldInfo )
		{
			_ilGenerator.Emit( OpCodes.Ldtoken, fieldInfo );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldtoken"/>, type) that
		/// converts a metadata token to its runtime representation, pushing it onto the evaluation stack.
		/// </summary>
		/// <param name="type">A Type</param>
		/// <seealso cref="OpCodes.Ldtoken">OpCodes.Ldtoken</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Type)">ILGenerator.Emit</seealso>
		public EmitHelper ldtoken( Type type )
		{
			_ilGenerator.Emit( OpCodes.Ldtoken, type );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ldvirtftn"/>, methodInfo) that
		/// pushes an unmanaged pointer (type natural int) to the native code implementing a particular virtual method 
		/// associated with a specified object onto the evaluation stack.
		/// </summary>
		/// <param name="methodInfo">The method to be called.</param>
		/// <seealso cref="OpCodes.Ldvirtftn">OpCodes.Ldvirtftn</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,MethodInfo)">ILGenerator.Emit</seealso>
		public EmitHelper ldvirtftn( MethodInfo methodInfo )
		{
			_ilGenerator.Emit( OpCodes.Ldvirtftn, methodInfo );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Leave"/>, label) that
		/// exits a protected region of code, unconditionally tranferring control to a specific target instruction.
		/// </summary>
		/// <param name="label">The label.</param>
		/// <seealso cref="OpCodes.Leave">OpCodes.Leave</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper leave( Label label )
		{
			_ilGenerator.Emit( OpCodes.Leave, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Leave_S"/>, label) that
		/// exits a protected region of code, unconditionally transferring control to a target instruction (short form).
		/// </summary>
		/// <param name="label">The label.</param>
		/// <seealso cref="OpCodes.Leave_S">OpCodes.Leave_S</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper leave_s( Label label )
		{
			_ilGenerator.Emit( OpCodes.Leave_S, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Localloc"/>) that
		/// allocates a certain number of bytes from the local dynamic memory pool and pushes the address 
		/// (a transient pointer, type *) of the first allocated byte onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Localloc">OpCodes.Localloc</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper localloc
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Localloc );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Mkrefany"/>, type) that
		/// pushes a typed reference to an instance of a specific type onto the evaluation stack.
		/// </summary>
		/// <param name="type">A Type</param>
		/// <seealso cref="OpCodes.Mkrefany">OpCodes.Mkrefany</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Type)">ILGenerator.Emit</seealso>
		public EmitHelper mkrefany( Type type )
		{
			_ilGenerator.Emit( OpCodes.Mkrefany, type );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Mul"/>) that
		/// multiplies two values and pushes the result on the evaluation stack.
		/// (a transient pointer, type *) of the first allocated byte onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Mul">OpCodes.Mul</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper mul
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Mul );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Mul_Ovf"/>) that
		/// multiplies two integer values, performs an overflow check, 
		/// and pushes the result onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Mul_Ovf">OpCodes.Mul_Ovf</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper mul_ovf
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Mul_Ovf );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Mul_Ovf_Un"/>) that
		/// multiplies two unsigned integer values, performs an overflow check, 
		/// and pushes the result onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Mul_Ovf_Un">OpCodes.Mul_Ovf_Un</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper mul_ovf_un
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Mul_Ovf_Un );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Neg"/>) that
		/// negates a value and pushes the result onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Neg">OpCodes.Neg</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper neg
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Neg );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Newarr"/>, type) that
		/// pushes an object reference to a new zero-based, one-dimensional array whose elements 
		/// are of a specific type onto the evaluation stack.
		/// </summary>
		/// <param name="type">A Type</param>
		/// <seealso cref="OpCodes.Newarr">OpCodes.Newarr</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Type)">ILGenerator.Emit</seealso>
		public EmitHelper newarr( Type type )
		{
			_ilGenerator.Emit( OpCodes.Newarr, type );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Newobj"/>, <see cref="ConstructorInfo"/>) that
		/// creates a new object or a new instance of a value type,
		/// pushing an object reference (type O) onto the evaluation stack.
		/// </summary>
		/// <param name="constructorInfo">A <see cref="ConstructorInfo"/> representing a constructor.</param>
		/// <seealso cref="OpCodes.Newobj">OpCodes.Newobj</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,ConstructorInfo)">ILGenerator.Emit</seealso>
		public EmitHelper newobj( ConstructorInfo constructorInfo )
		{
			_ilGenerator.Emit( OpCodes.Newobj, constructorInfo );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Newobj"/>, ConstructorInfo) that
		/// creates a new object or a new instance of a value type,
		/// pushing an object reference (type O) onto the evaluation stack.
		/// </summary>
		/// <param name="type">A type.</param>
		/// <param name="parameters">An array of System.Type objects representing
		/// the number, order, and type of the parameters for the desired constructor.
		/// -or- An empty array of System.Type objects, to get a constructor that takes
		/// no parameters. Such an empty array is provided by the static field System.Type.EmptyTypes.</param>
		public EmitHelper newobj( Type type, params Type[] parameters )
		{
			if( type == null )
			{
				throw new ArgumentNullException( "type" );
			}

			ConstructorInfo ci = type.GetConstructor( parameters );

			return newobj( ci );
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Nop"/>) that
		/// fills space if opcodes are patched. No meaningful operation is performed although 
		/// a processing cycle can be consumed.
		/// </summary>
		/// <seealso cref="OpCodes.Nop">OpCodes.Nop</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper nop
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Nop );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Not"/>) that
		/// computes the bitwise complement of the integer value on top of the stack 
		/// and pushes the result onto the evaluation stack as the same type.
		/// </summary>
		/// <seealso cref="OpCodes.Not">OpCodes.Not</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper not
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Not );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Or"/>) that
		/// compute the bitwise complement of the two integer values on top of the stack and 
		/// pushes the result onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Or">OpCodes.Or</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper or
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Or );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Pop"/>) that
		/// removes the value currently on top of the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Pop">OpCodes.Pop</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper pop
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Pop );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Refanytype"/>) that
		/// specifies that the subsequent array address operation performs
		/// no type check at run time, and that it returns a managed pointer
		/// whose mutability is restricted.
		/// </summary>
		/// <seealso cref="OpCodes.Refanytype">OpCodes.Refanytype</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper @readonly
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Readonly );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Refanytype"/>) that
		/// retrieves the type token embedded in a typed reference.
		/// </summary>
		/// <seealso cref="OpCodes.Refanytype">OpCodes.Refanytype</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper refanytype
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Refanytype );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Refanyval"/>, type) that
		/// retrieves the address (type &amp;) embedded in a typed reference.
		/// </summary>
		/// <param name="type">A Type</param>
		/// <seealso cref="OpCodes.Refanyval">OpCodes.Refanyval</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Type)">ILGenerator.Emit</seealso>
		public EmitHelper refanyval( Type type )
		{
			_ilGenerator.Emit( OpCodes.Refanyval, type );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Rem"/>) that
		/// divides two values and pushes the remainder onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Rem">OpCodes.Rem</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper rem
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Rem );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Rem_Un"/>) that
		/// divides two unsigned values and pushes the remainder onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Rem_Un">OpCodes.Rem_Un</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper rem_un
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Rem_Un );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Ret"/>) that
		/// returns from the current method, pushing a return value (if present) 
		/// from the caller's evaluation stack onto the callee's evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Ret">OpCodes.Ret</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper ret()
		{
			_ilGenerator.Emit( OpCodes.Ret );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Rethrow"/>) that
		/// rethrows the current exception.
		/// </summary>
		/// <seealso cref="OpCodes.Rethrow">OpCodes.Rethrow</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper rethrow
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Rethrow );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Shl"/>) that
		/// shifts an integer value to the left (in zeroes) by a specified number of bits,
		/// pushing the result onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Shl">OpCodes.Shl</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper shl
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Shl );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Shr"/>) that
		/// shifts an integer value (in sign) to the right by a specified number of bits,
		/// pushing the result onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Shr">OpCodes.Shr</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper shr
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Shr );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Shr_Un"/>) that
		/// shifts an unsigned integer value (in zeroes) to the right by a specified number of bits,
		/// pushing the result onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Shr_Un">OpCodes.Shr_Un</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper shr_un
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Shr_Un );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Sizeof"/>, type) that
		/// pushes the size, in bytes, of a supplied value type onto the evaluation stack.
		/// </summary>
		/// <param name="type">A Type</param>
		/// <seealso cref="OpCodes.Sizeof">OpCodes.Sizeof</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Type)">ILGenerator.Emit</seealso>
		public EmitHelper @sizeof( Type type )
		{
			_ilGenerator.Emit( OpCodes.Sizeof, type );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Starg"/>, short) that
		/// stores the value on top of the evaluation stack in the argument slot at a specified index.
		/// </summary>
		/// <param name="index">Slot index.</param>
		/// <seealso cref="OpCodes.Starg">OpCodes.Starg</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,short)">ILGenerator.Emit</seealso>
		public EmitHelper starg( short index )
		{
			_ilGenerator.Emit( OpCodes.Starg, index );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Starg_S"/>, byte) that
		/// stores the value on top of the evaluation stack in the argument slot at a specified index,
		/// short form.
		/// </summary>
		/// <param name="index">Slot index.</param>
		/// <seealso cref="OpCodes.Starg_S">OpCodes.Starg_S</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,byte)">ILGenerator.Emit</seealso>
		public EmitHelper starg_s( byte index )
		{
			_ilGenerator.Emit( OpCodes.Starg_S, index );
			return this;
		}

		/// <summary>
		/// Stores the value on top of the evaluation stack in the argument slot at a specified index.
		/// </summary>
		/// <param name="index">Slot index.</param>
		/// <seealso cref="OpCodes.Starg">OpCodes.Starg</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,short)">ILGenerator.Emit</seealso>
		public EmitHelper starg( int index )
		{
			if( index < byte.MaxValue )
			{
				starg_s( (byte) index );
			}
			else if( index < short.MaxValue )
			{
				starg( (short) index );
			}
			else
			{
				throw new ArgumentOutOfRangeException( "index" );
			}

			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Stelem_I"/>) that
		/// replaces the array element at a given index with the natural int value 
		/// on the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Stelem_I">OpCodes.Stelem_I</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper stelem_i
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Stelem_I );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Stelem_I1"/>) that
		/// replaces the array element at a given index with the int8 value on the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Stelem_I1">OpCodes.Stelem_I1</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper stelem_i1
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Stelem_I1 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Stelem_I2"/>) that
		/// replaces the array element at a given index with the int16 value on the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Stelem_I2">OpCodes.Stelem_I2</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper stelem_i2
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Stelem_I2 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Stelem_I4"/>) that
		/// replaces the array element at a given index with the int32 value on the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Stelem_I4">OpCodes.Stelem_I4</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper stelem_i4
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Stelem_I4 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Stelem_I8"/>) that
		/// replaces the array element at a given index with the int64 value on the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Stelem_I8">OpCodes.Stelem_I8</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper stelem_i8
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Stelem_I8 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Stelem_R4"/>) that
		/// replaces the array element at a given index with the float32 value on the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Stelem_R4">OpCodes.Stelem_R4</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper stelem_r4
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Stelem_R4 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Stelem_R8"/>) that
		/// replaces the array element at a given index with the float64 value on the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Stelem_R8">OpCodes.Stelem_R8</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper stelem_r8
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Stelem_R8 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Stelem_Ref"/>) that
		/// replaces the array element at a given index with the object ref value (type O)
		/// on the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Stelem_Ref">OpCodes.Stelem_Ref</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper stelem_ref
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Stelem_Ref );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Stfld"/>, <see cref="FieldInfo"/>) that
		/// replaces the value stored in the field of an object reference or pointer with a new value.
		/// </summary>
		/// <param name="fieldInfo">A <see cref="FieldInfo"/> representing a field.</param>
		/// <seealso cref="OpCodes.Stfld">OpCodes.Stfld</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,FieldInfo)">ILGenerator.Emit</seealso>
		public EmitHelper stfld( FieldInfo fieldInfo )
		{
			_ilGenerator.Emit( OpCodes.Stfld, fieldInfo );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Stind_I"/>) that
		/// stores a value of type natural int at a supplied address.
		/// </summary>
		/// <seealso cref="OpCodes.Stind_I">OpCodes.Stind_I</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper stind_i
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Stind_I );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Stind_I1"/>) that
		/// stores a value of type int8 at a supplied address.
		/// </summary>
		/// <seealso cref="OpCodes.Stind_I1">OpCodes.Stind_I1</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper stind_i1
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Stind_I1 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Stind_I2"/>) that
		/// stores a value of type int16 at a supplied address.
		/// </summary>
		/// <seealso cref="OpCodes.Stind_I2">OpCodes.Stind_I2</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper stind_i2
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Stind_I2 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Stind_I4"/>) that
		/// stores a value of type int32 at a supplied address.
		/// </summary>
		/// <seealso cref="OpCodes.Stind_I4">OpCodes.Stind_I4</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper stind_i4
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Stind_I4 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Stind_I8"/>) that
		/// stores a value of type int64 at a supplied address.
		/// </summary>
		/// <seealso cref="OpCodes.Stind_I8">OpCodes.Stind_I8</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper stind_i8
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Stind_I8 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Stind_R4"/>) that
		/// stores a value of type float32 at a supplied address.
		/// </summary>
		/// <seealso cref="OpCodes.Stind_R4">OpCodes.Stind_R4</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper stind_r4
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Stind_R4 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Stind_R8"/>) that
		/// stores a value of type float64 at a supplied address.
		/// </summary>
		/// <seealso cref="OpCodes.Stind_R8">OpCodes.Stind_R8</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper stind_r8
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Stind_R8 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Stind_Ref"/>) that
		/// stores an object reference value at a supplied address.
		/// </summary>
		/// <seealso cref="OpCodes.Stind_Ref">OpCodes.Stind_Ref</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper stind_ref
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Stind_Ref );
				return this;
			}
		}

		/// <summary>
		/// Stores a value of the type at a supplied address.
		/// </summary>
		/// <param name="type">A Type.</param>
		public EmitHelper stind( Type type )
		{
			if( type == null )
			{
				throw new ArgumentNullException( "type" );
			}

			switch( Type.GetTypeCode( type ) )
			{
				case TypeCode.Boolean:
				case TypeCode.Byte:
				case TypeCode.SByte:
					stind_i1.end();
					break;

				case TypeCode.Char:
				case TypeCode.Int16:
				case TypeCode.UInt16:
					stind_i2.end();
					break;

				case TypeCode.Int32:
				case TypeCode.UInt32:
					stind_i4.end();
					break;

				case TypeCode.Int64:
				case TypeCode.UInt64:
					stind_i8.end();
					break;

				case TypeCode.Single:
					stind_r4.end();
					break;
				case TypeCode.Double:
					stind_r8.end();
					break;

				default:
					if( type.IsClass )
					{
						stind_ref.end();
					}
					else if( type.IsValueType )
					{
						stobj( type );
					}
					else
					{
						throw CreateNotExpectedTypeException( type );
					}
					break;
			}

			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Stloc"/>, <see cref="LocalBuilder"/>) that
		/// pops the current value from the top of the evaluation stack and stores it 
		/// in the local variable list at a specified index.
		/// </summary>
		/// <param name="local">A local variable.</param>
		/// <seealso cref="OpCodes.Stloc">OpCodes.Stloc</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,LocalBuilder)">ILGenerator.Emit</seealso>
		public EmitHelper stloc( LocalBuilder local )
		{
			_ilGenerator.Emit( OpCodes.Stloc, local );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Stloc"/>, short) that
		/// pops the current value from the top of the evaluation stack and stores it 
		/// in the local variable list at a specified index.
		/// </summary>
		/// <param name="index">A local variable index.</param>
		/// <seealso cref="OpCodes.Stloc">OpCodes.Stloc</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,short)">ILGenerator.Emit</seealso>
		public EmitHelper stloc( short index )
		{
			if( index >= byte.MinValue && index <= byte.MaxValue )
			{
				return stloc_s( (byte) index );
			}

			_ilGenerator.Emit( OpCodes.Stloc, index );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Stloc_0"/>) that
		/// pops the current value from the top of the evaluation stack and stores it 
		/// in the local variable list at index 0.
		/// </summary>
		/// <seealso cref="OpCodes.Stloc_0">OpCodes.Stloc_0</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper stloc_0
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Stloc_0 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Stloc_1"/>) that
		/// pops the current value from the top of the evaluation stack and stores it 
		/// in the local variable list at index 1.
		/// </summary>
		/// <seealso cref="OpCodes.Stloc_1">OpCodes.Stloc_1</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper stloc_1
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Stloc_1 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Stloc_2"/>) that
		/// pops the current value from the top of the evaluation stack and stores it 
		/// in the local variable list at index 2.
		/// </summary>
		/// <seealso cref="OpCodes.Stloc_2">OpCodes.Stloc_2</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper stloc_2
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Stloc_2 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Stloc_3"/>) that
		/// pops the current value from the top of the evaluation stack and stores it 
		/// in the local variable list at index 3.
		/// </summary>
		/// <seealso cref="OpCodes.Stloc_3">OpCodes.Stloc_3</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper stloc_3
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Stloc_3 );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Stloc_S"/>, <see cref="LocalBuilder"/>) that
		/// pops the current value from the top of the evaluation stack and stores it 
		/// in the local variable list at index (short form).
		/// </summary>
		/// <param name="local">A local variable.</param>
		/// <seealso cref="OpCodes.Stloc_S">OpCodes.Stloc_S</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,LocalBuilder)">ILGenerator.Emit</seealso>
		public EmitHelper stloc_s( LocalBuilder local )
		{
			_ilGenerator.Emit( OpCodes.Stloc_S, local );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Stloc_S"/>, byte) that
		/// pops the current value from the top of the evaluation stack and stores it 
		/// in the local variable list at index (short form).
		/// </summary>
		/// <param name="index">A local variable index.</param>
		/// <seealso cref="OpCodes.Stloc_S">OpCodes.Stloc_S</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,short)">ILGenerator.Emit</seealso>
		public EmitHelper stloc_s( byte index )
		{
			switch( index )
			{
				case 0:
					stloc_0.end();
					break;
				case 1:
					stloc_1.end();
					break;
				case 2:
					stloc_2.end();
					break;
				case 3:
					stloc_3.end();
					break;

				default:
					_ilGenerator.Emit( OpCodes.Stloc_S, index );
					break;
			}

			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Stobj"/>, type) that
		/// copies a value of a specified type from the evaluation stack into a supplied memory address.
		/// </summary>
		/// <param name="type">A Type</param>
		/// <seealso cref="OpCodes.Stobj">OpCodes.Stobj</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Type)">ILGenerator.Emit</seealso>
		public EmitHelper stobj( Type type )
		{
			_ilGenerator.Emit( OpCodes.Stobj, type );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Stsfld"/>, fieldInfo) that
		/// replaces the value of a static field with a value from the evaluation stack.
		/// </summary>
		/// <param name="fieldInfo">A <see cref="FieldInfo"/> representing a field.</param>
		/// <seealso cref="OpCodes.Stsfld">OpCodes.Stsfld</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,FieldInfo)">ILGenerator.Emit</seealso>
		public EmitHelper stsfld( FieldInfo fieldInfo )
		{
			_ilGenerator.Emit( OpCodes.Stsfld, fieldInfo );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Sub"/>) that
		/// subtracts one value from another and pushes the result onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Sub">OpCodes.Sub</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper sub
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Sub );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Sub_Ovf"/>) that
		/// subtracts one integer value from another, performs an overflow check,
		/// and pushes the result onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Sub_Ovf">OpCodes.Sub_Ovf</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper sub_ovf
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Sub_Ovf );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Sub_Ovf_Un"/>) that
		/// subtracts one unsigned integer value from another, performs an overflow check,
		/// and pushes the result onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Sub_Ovf_Un">OpCodes.Sub_Ovf_Un</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper sub_ovf_un
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Sub_Ovf_Un );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Switch"/>, label[]) that
		/// implements a jump table.
		/// </summary>
		/// <param name="labels">The array of label objects to which to branch from this location.</param>
		/// <seealso cref="OpCodes.Switch">OpCodes.Switch</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label[])">ILGenerator.Emit</seealso>
		public EmitHelper @switch( Label[] labels )
		{
			_ilGenerator.Emit( OpCodes.Switch, labels );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Tailcall"/>) that
		/// performs a postfixed method call instruction such that the current method's stack frame 
		/// is removed before the actual call instruction is executed.
		/// </summary>
		/// <seealso cref="OpCodes.Tailcall">OpCodes.Tailcall</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper tailcall
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Tailcall );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Throw"/>) that
		/// throws the exception object currently on the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Throw">OpCodes.Throw</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper @throw
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Throw );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Unaligned"/>, label) that
		/// indicates that an address currently atop the evaluation stack might not be aligned 
		/// to the natural size of the immediately following ldind, stind, ldfld, stfld, ldobj, stobj, 
		/// initblk, or cpblk instruction.
		/// </summary>
		/// <param name="label">The label to branch from this location.</param>
		/// <seealso cref="OpCodes.Unaligned">OpCodes.Unaligned</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Label)">ILGenerator.Emit</seealso>
		public EmitHelper unaligned( Label label )
		{
			_ilGenerator.Emit( OpCodes.Unaligned, label );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Unaligned"/>, long) that
		/// indicates that an address currently atop the evaluation stack might not be aligned 
		/// to the natural size of the immediately following ldind, stind, ldfld, stfld, ldobj, stobj, 
		/// initblk, or cpblk instruction.
		/// </summary>
		/// <param name="addr">An address is pushed onto the stack.</param>
		/// <seealso cref="OpCodes.Unaligned">OpCodes.Unaligned</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,long)">ILGenerator.Emit</seealso>
		public EmitHelper unaligned( long addr )
		{
			_ilGenerator.Emit( OpCodes.Unaligned, addr );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Unbox"/>, type) that
		/// converts the boxed representation of a value type to its unboxed form.
		/// </summary>
		/// <param name="type">A Type</param>
		/// <seealso cref="OpCodes.Unbox">OpCodes.Unbox</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Type)">ILGenerator.Emit</seealso>
		public EmitHelper unbox( Type type )
		{
			_ilGenerator.Emit( OpCodes.Unbox, type );
			return this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Unbox_Any"/>, type) that
		/// converts the boxed representation of a value type to its unboxed form.
		/// </summary>
		/// <param name="type">A Type</param>
		/// <seealso cref="OpCodes.Unbox_Any">OpCodes.Unbox_Any</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Type)">ILGenerator.Emit</seealso>
		public EmitHelper unbox_any( Type type )
		{
			_ilGenerator.Emit( OpCodes.Unbox_Any, type );
			return this;
		}

		/// <summary>
		/// Calls <see cref="unbox_any(Type)"/> if given type is a value type.
		/// </summary>
		/// <param name="type">A Type</param>
		/// <seealso cref="OpCodes.Unbox_Any">OpCodes.Unbox</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode,Type)">ILGenerator.Emit</seealso>
		public EmitHelper unboxIfValueType( Type type )
		{
			if( type == null )
			{
				throw new ArgumentNullException( "type" );
			}

			return type.IsValueType ? unbox_any( type ) : this;
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Volatile"/>) that
		/// specifies that an address currently atop the evaluation stack might be volatile, 
		/// and the results of reading that location cannot be cached or that multiple stores 
		/// to that location cannot be suppressed.
		/// </summary>
		/// <seealso cref="OpCodes.Volatile">OpCodes.Volatile</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper @volatile
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Volatile );
				return this;
			}
		}

		/// <summary>
		/// Calls ILGenerator.Emit(<see cref="OpCodes.Xor"/>) that
		/// computes the bitwise XOR of the top two values on the evaluation stack, 
		/// pushing the result onto the evaluation stack.
		/// </summary>
		/// <seealso cref="OpCodes.Xor">OpCodes.Xor</seealso>
		/// <seealso cref="System.Reflection.Emit.ILGenerator.Emit(OpCode)">ILGenerator.Emit</seealso>
		public EmitHelper xor
		{
			get
			{
				_ilGenerator.Emit( OpCodes.Xor );
				return this;
			}
		}

		/// <summary>
		/// Ends sequence of property calls.
		/// </summary>
		[SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic" )]
		public void end()
		{
		}
		#endregion

		/// <summary>
		/// Loads default value of given type onto the evaluation stack.
		/// </summary>
		/// <param name="type">A Type</param>
		public EmitHelper LoadInitValue( Type type )
		{
			if( type == null )
			{
				throw new ArgumentNullException( "type" );
			}

			switch( Type.GetTypeCode( type ) )
			{
				case TypeCode.Boolean:
				case TypeCode.Char:
				case TypeCode.SByte:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Byte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
					ldc_i4_0.end();
					break;

				case TypeCode.Int64:
				case TypeCode.UInt64:
					ldc_i4_0.conv_i8.end();
					break;

				case TypeCode.Single:
				case TypeCode.Double:
					ldc_r4( 0 ).end();
					break;

				case TypeCode.String:
					ldsfld( typeof(string).GetField( "Empty" ) );
					break;

				default:
					if( type.IsClass || type.IsInterface )
					{
						ldnull.end();
					}
					else
					{
						throw CreateNotExpectedTypeException( type );
					}
					break;
			}

			return this;
		}

		/// <summary>
		/// Loads supplied object value (if possible) onto the evaluation stack.
		/// </summary>
		/// <param name="o">Any object instance or null reference.</param>
		/// <returns>True is a value was loaded, otherwise false.</returns>
		public bool LoadWellKnownValue( object o )
		{
			if( o == null )
			{
				ldnull.end();
			}
			else
			{
				switch( Type.GetTypeCode( o.GetType() ) )
				{
					case TypeCode.Boolean:
						ldc_bool( (Boolean) o );
						break;
					case TypeCode.Char:
						ldc_i4_( (Char) o );
						break;
					case TypeCode.Single:
						ldc_r4( (Single) o );
						break;
					case TypeCode.Double:
						ldc_r8( (Double) o );
						break;
					case TypeCode.String:
						ldstr( (String) o );
						break;
					case TypeCode.SByte:
						ldc_i4_( (SByte) o );
						break;
					case TypeCode.Int16:
						ldc_i4_( (Int16) o );
						break;
					case TypeCode.Int32:
						ldc_i4_( (Int32) o );
						break;
					case TypeCode.Int64:
						ldc_i8( (Int64) o );
						break;
					case TypeCode.Byte:
						ldc_i4_( (Byte) o );
						break;
					case TypeCode.UInt16:
						ldc_i4_( (UInt16) o );
						break;
					case TypeCode.UInt32:
						ldc_i4_( unchecked((Int32) (UInt32) o) );
						break;
					case TypeCode.UInt64:
						ldc_i8( unchecked((Int64) (UInt64) o) );
						break;
					default:
						return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Initialize local variable with some default value.
		/// </summary>
		/// <param name="localBuilder">A method local variable.</param>
		public EmitHelper Init( LocalBuilder localBuilder )
		{
			if( localBuilder == null )
			{
				throw new ArgumentNullException( "localBuilder" );
			}

			Type type = localBuilder.LocalType;

			if( type.IsEnum )
			{
				type = Enum.GetUnderlyingType( type );
			}

			return type.IsValueType && type.IsPrimitive == false
			       	? ldloca( localBuilder ).initobj( type )
			       	: LoadInitValue( type ).stloc( localBuilder );
		}

		/// <summary>
		/// Loads a type instance at runtime.
		/// </summary>
		/// <param name="type">A type</param>
		public EmitHelper LoadType( Type type )
		{
			return type == null
			       	? ldnull
			       	: ldtoken( type ).call( typeof(Type), "GetTypeFromHandle", typeof(RuntimeTypeHandle) );
		}

		/// <summary>
		/// Loads a field instance at runtime.
		/// </summary>
		/// <param name="fieldInfo">A <see cref="FieldInfo"/> representing a field.</param>
		public EmitHelper LoadField( FieldInfo fieldInfo )
		{
			return fieldInfo.IsStatic ? ldsfld( fieldInfo ) : ldarg_0.ldfld( fieldInfo );
		}

		/// <summary>
		/// Cast an object passed by reference to the specified type
		/// or unbox a boxed value type.
		/// </summary>
		/// <param name="type">A type</param>
		public EmitHelper CastFromObject( Type type )
		{
			if( type == null )
			{
				throw new ArgumentNullException( "type" );
			}

			return
				type == typeof(object)
					? this
					: (type.IsValueType
					   	? unbox_any( type )
					   	: castclass( type ));
		}

		/// <summary>
		/// Increase max stack size by specified delta.
		/// </summary>
		/// <param name="size">Number of bytes to enlarge max stack size.</param>
		public void AddMaxStackSize( int size )
		{
			// m_maxStackSize isn't public so we need some hacking here.
			//
			FieldInfo fi = _ilGenerator.GetType().GetField(
				"m_maxStackSize", BindingFlags.Instance | BindingFlags.NonPublic );

			if( fi != null )
			{
				size += (int) fi.GetValue( _ilGenerator );
				fi.SetValue( _ilGenerator, size );
			}
		}

		private static Exception CreateNoSuchMethodException( Type type, string methodName )
		{
			return new InvalidOperationException(
				string.Format( "Method {1} cannot be found in type {0}", type.FullName, methodName ) );
		}

		private static Exception CreateNotExpectedTypeException( Type type )
		{
			return new ArgumentException(
				string.Format( "Type {0} is not expected in this context", type.FullName ) );
		}
	}
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Emitter\InvocationEmitter.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect.Emitter
{
    using System;
    using System.Reflection;

    internal abstract class InvocationEmitter : BaseEmitter
    {
        protected InvocationEmitter(CallInfo callInfo)
            : base(callInfo)
        {
        }

        protected byte CreateLocalsForByRefParams( byte paramArrayIndex, MethodBase invocationInfo )
        {
            byte numberOfByRefParams = 0;
            var parameters = invocationInfo.GetParameters();
            for( int i = 0; i < CallInfo.ParamTypes.Length; i++ )
            {
                Type paramType = CallInfo.ParamTypes[ i ];
                if( paramType.IsByRef )
                {
                    Type type = paramType.GetElementType();
                    Generator.DeclareLocal( type );
                    if( !parameters[ i ].IsOut ) // no initialization necessary is 'out' parameter
                    {
                        Generator.ldarg( paramArrayIndex )
                            .ldc_i4( i )
                            .ldelem_ref
                            .CastFromObject( type )
                            .stloc( numberOfByRefParams )
                            .end();
                    }
                    numberOfByRefParams++;
                }
            }
            return numberOfByRefParams;
        }

        protected void AssignByRefParamsToArray( int paramArrayIndex )
        {
            byte currentByRefParam = 0;
            for( int i = 0; i < CallInfo.ParamTypes.Length; i++ )
            {
                Type paramType = CallInfo.ParamTypes[ i ];
                if( paramType.IsByRef )
                {
                    Generator.ldarg( paramArrayIndex )
                        .ldc_i4( i )
                        .ldloc( currentByRefParam++ )
                        .boxIfValueType( paramType.GetElementType() )
                        .stelem_ref
                        .end();
                }
            }
        }

        protected void PushParamsOrLocalsToStack( int paramArrayIndex )
        {
            byte currentByRefParam = 0;
            for( int i = 0; i < CallInfo.ParamTypes.Length; i++ )
            {
                Type paramType = CallInfo.ParamTypes[ i ];
                if( paramType.IsByRef )
                {
                    Generator.ldloca_s( currentByRefParam++ );
                }
                else
                {
                    Generator.ldarg( paramArrayIndex )
                        .ldc_i4( i )
                        .ldelem_ref
                        .CastFromObject( paramType );
                }
            }
        }
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Emitter\LookupUtils.cs
//------------------------------------------------------------------------------
#region License

// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/

#endregion


namespace Fasterflect.Emitter
{
    using System;
    using System.Reflection;

    internal class LookupUtils
    {
        public static ConstructorInfo GetConstructor(CallInfo callInfo)
        {
            var constructor = callInfo.MemberInfo as ConstructorInfo;
            if (constructor != null)
                return constructor;

            constructor = callInfo.TargetType.Constructor( callInfo.BindingFlags, callInfo.ParamTypes );
            if (constructor == null)
                throw new MissingMemberException("Constructor does not exist");
			callInfo.MemberInfo = constructor;
        	callInfo.ParamTypes = constructor.GetParameters().ToTypeArray();
            return constructor;
        }

        public static MethodInfo GetMethod(CallInfo callInfo)
        {
            var method = callInfo.MemberInfo as MethodInfo;
            if (method != null)
                return method;
            method = callInfo.TargetType.Method(callInfo.GenericTypes, callInfo.Name, callInfo.ParamTypes, callInfo.BindingFlags);
            if (method == null)
			{
				const string fmt = "No match for method with name {0} and flags {1} on type {2}.";
				throw new MissingMethodException( string.Format( fmt, callInfo.Name, callInfo.BindingFlags, callInfo.TargetType ) );
			}
			callInfo.MemberInfo = method;
        	callInfo.ParamTypes = method.GetParameters().ToTypeArray();
            return method;
        }

        public static MemberInfo GetMember(CallInfo callInfo)
        {
            var member = callInfo.MemberInfo;
            if (member != null)
                return member;

            if (callInfo.MemberTypes == MemberTypes.Property)
            {
                member = callInfo.TargetType.Property(callInfo.Name, callInfo.BindingFlags);
                if (member == null)
				{
					const string fmt = "No match for property with name {0} and flags {1} on type {2}.";
					throw new MissingMemberException( string.Format( fmt, callInfo.Name, callInfo.BindingFlags, callInfo.TargetType ) );
				}
				callInfo.MemberInfo = member;
				return member;
            }
            if (callInfo.MemberTypes == MemberTypes.Field)
            {
                member = callInfo.TargetType.Field(callInfo.Name, callInfo.BindingFlags);
                if (member == null)
				{
					const string fmt = "No match for field with name {0} and flags {1} on type {2}.";
					throw new MissingFieldException( string.Format( fmt, callInfo.Name, callInfo.BindingFlags, callInfo.TargetType ) );
				}
				callInfo.MemberInfo = member;
                return member;
            }
            throw new ArgumentException(callInfo.MemberTypes + " is not supported");
        }

		public static FieldInfo GetField( CallInfo callInfo )
		{
			var field = callInfo.TargetType.Field( callInfo.Name, callInfo.BindingFlags );
			if( field == null )
			{
				const string fmt = "No match for field with name {0} and flags {1} on type {2}.";
				throw new MissingFieldException( string.Format( fmt, callInfo.Name, callInfo.BindingFlags, callInfo.TargetType ) );
			}
			callInfo.MemberInfo = field;
			return field;
		}

		public static PropertyInfo GetProperty( CallInfo callInfo )
		{
			var property = callInfo.TargetType.Property( callInfo.Name, callInfo.BindingFlags );
			if( property == null )
			{
				const string fmt = "No match for property with name {0} and flags {1} on type {2}.";
				throw new MissingMemberException( string.Format( fmt, callInfo.Name, callInfo.BindingFlags, callInfo.TargetType ) );
			}
			callInfo.MemberInfo = property;
			return property;
		}

    	public static MethodInfo GetPropertyGetMethod(PropertyInfo propInfo, CallInfo callInfo)
        {
            var method = propInfo.GetGetMethod();
			if( method != null )
			    callInfo.MemberInfo = method;
			return method ?? GetPropertyMethod("get_", "getter", callInfo);
        }

        public static MethodInfo GetPropertySetMethod(PropertyInfo propInfo, CallInfo callInfo)
        {
            var method = propInfo.GetSetMethod();
			if( method != null )
			    callInfo.MemberInfo = method;
            return method ?? GetPropertyMethod("set_", "setter", callInfo);
        }

        private static MethodInfo GetPropertyMethod(string infoPrefix, string propertyMethod, CallInfo callInfo)
        {
            var method = callInfo.TargetType.Method(infoPrefix + callInfo.Name, callInfo.BindingFlags);
            if (method == null)
			{
				const string fmt = "No {0} for property {1} with flags {2} on type {3}.";
				throw new MissingFieldException( string.Format( fmt, propertyMethod, callInfo.Name, callInfo.BindingFlags, callInfo.TargetType ) );
			}
			callInfo.MemberInfo = method;
            return method;
        }
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Emitter\MapCallInfo.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect.Emitter
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Stores all necessary information to construct a dynamic method for member mapping.
    /// </summary>
    [DebuggerStepThrough]
    internal class MapCallInfo : CallInfo
    {
    	public Type SourceType { get; private set; }
        public MemberTypes SourceMemberTypes { get; private set; }
        public MemberTypes TargetMemberTypes { get; private set; }
        public string[] Names { get; private set; }

    	public MapCallInfo( Type targetType, Type[] genericTypes, Flags bindingFlags, MemberTypes memberTypes, string name, Type[] parameterTypes, MemberInfo memberInfo, bool isReadOperation, Type sourceType, MemberTypes sourceMemberTypes, MemberTypes targetMemberTypes, string[] names ) : base( targetType, genericTypes, bindingFlags, memberTypes, name, parameterTypes, memberInfo, isReadOperation )
    	{
    		SourceType = sourceType;
    		SourceMemberTypes = sourceMemberTypes;
    		TargetMemberTypes = targetMemberTypes;
    		Names = names;
    	}

    	public override bool Equals( object obj )
        {
            var other = obj as MapCallInfo;
            if( other == null )
            {
                return false;
            }
            if( ! base.Equals( obj ) )
            {
                return false;
            }
            if( other.SourceType != SourceType ||
                other.SourceMemberTypes != SourceMemberTypes ||
                other.TargetMemberTypes != TargetMemberTypes ||
                (other.Names == null && Names != null) ||
				(other.Names != null && Names == null) ||
				(other.Names != null && Names != null && other.Names.Length != Names.Length) )
            {
                return false;
            }
            if( other.Names != null && Names != null )
            {
            	for( int i = 0; i < Names.Length; i++ )
            	{
            		if( Names[ i ] != other.Names[ i ] )
            		{
            			return false;
            		}
            	}
            }
    		return true;
        }

        public override int GetHashCode()
        {
        	int hash = base.GetHashCode() + SourceType.GetHashCode() * SourceMemberTypes.GetHashCode() * TargetMemberTypes.GetHashCode();
			for( int i = 0; i < Names.Length; i++ )
            {
                hash += Names[ i ].GetHashCode() * (i+1);
            }
            return hash;
        }        
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Emitter\MapEmitter.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect.Emitter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    internal class MapEmitter : BaseEmitter
    {
        private readonly Type sourceType;
        private readonly MemberTypes sourceMemberTypes;
        private readonly MemberTypes targetMemberTypes;
        private readonly string[] names;

        public MapEmitter(Type sourceType, Type targetType, MemberTypes sourceMemberTypes, MemberTypes targetMemberTypes,
                           Flags bindingFlags, params string[] names)
            : base(new MapCallInfo(targetType, null, 
                // Auto-apply IgnoreCase if we're mapping from one membertype to another
                Flags.SetIf(bindingFlags, Flags.IgnoreCase, (sourceMemberTypes & targetMemberTypes) != sourceMemberTypes), 
                MemberTypes.Custom, 
                "Fasterflect_Map", 
                Type.EmptyTypes, 
                null,
				false, 
				sourceType, 
				sourceMemberTypes, 
				targetMemberTypes, 
				names))
        {
            this.sourceType = sourceType;
            this.sourceMemberTypes = sourceMemberTypes;
            this.targetMemberTypes = targetMemberTypes;
            this.names = names;
        }

        protected internal override DynamicMethod CreateDynamicMethod()
        {
            return CreateDynamicMethod(sourceType.Name, sourceType, null, new[] { Constants.ObjectType, Constants.ObjectType });
        }

        protected internal override Delegate CreateDelegate()
        {
            bool handleInnerStruct = CallInfo.ShouldHandleInnerStruct;
            if (handleInnerStruct)
            {
                Generator.ldarg_1.end();                     // load arg-1 (target)
                Generator.DeclareLocal(CallInfo.TargetType); // TargetType localStr;
                Generator
                    .castclass(Constants.StructType) // (ValueTypeHolder)wrappedStruct
                    .callvirt(StructGetMethod) // <stack>.get_Value()
                    .unbox_any(CallInfo.TargetType) // unbox <stack>
                    .stloc(0); // localStr = <stack>
            }

            foreach (var pair in GetMatchingMembers())
            {
                if (handleInnerStruct)
                    Generator.ldloca_s(0).end(); // load &localStr
                else
                    Generator.ldarg_1.castclass(CallInfo.TargetType).end(); // ((TargetType)target)
                Generator.ldarg_0.castclass(sourceType);
                GenerateGetMemberValue(pair.Key);
                GenerateSetMemberValue(pair.Value);
            }

            if (handleInnerStruct)
            {
                StoreLocalToInnerStruct(1, 0);     // ((ValueTypeHolder)this)).Value = tmpStr
            }

            Generator.ret();
            return Method.CreateDelegate(typeof(ObjectMapper));
        }

        private void GenerateGetMemberValue(MemberInfo member)
        {
            if (member is FieldInfo)
            {
                Generator.ldfld((FieldInfo)member);
            }
            else
            {
                var method = ((PropertyInfo)member).GetGetMethod(true);
                Generator.callvirt(method, null);
            }
        }

        private void GenerateSetMemberValue(MemberInfo member)
        {
            if (member is FieldInfo)
            {
                Generator.stfld((FieldInfo)member);
            }
            else
            {
                var method = ((PropertyInfo)member).GetSetMethod(true);
                Generator.callvirt(method, null);
            }
        }

        private IEnumerable<KeyValuePair<MemberInfo, MemberInfo>> GetMatchingMembers()
        {
            StringComparison comparison = CallInfo.BindingFlags.IsSet(Flags.IgnoreCase)
                                            ? StringComparison.OrdinalIgnoreCase
                                            : StringComparison.Ordinal;
            var query = from s in sourceType.Members(sourceMemberTypes, CallInfo.BindingFlags, names)
                        from t in CallInfo.TargetType.Members(targetMemberTypes, CallInfo.BindingFlags, names)
                        where s.Name.Equals(t.Name, comparison) &&
                              t.Type().IsAssignableFrom(s.Type()) &&
                              s.IsReadable() && t.IsWritable()
                        select new { Source = s, Target = t };
            return query.ToDictionary(k => k.Source, v => v.Target);
        }
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Emitter\MemberGetEmitter.cs
//------------------------------------------------------------------------------
#region License

// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/

#endregion


namespace Fasterflect.Emitter
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

	internal class MemberGetEmitter : BaseEmitter
    {
        public MemberGetEmitter( MemberInfo memberInfo, Flags bindingFlags )
            : this( memberInfo.DeclaringType, bindingFlags, memberInfo.MemberType, memberInfo.Name, memberInfo )
        {
        }

        public MemberGetEmitter( Type targetType, Flags bindingFlags, MemberTypes memberType, string fieldOrPropertyName )
            : this( targetType, bindingFlags, memberType, fieldOrPropertyName, null )
        {
        }

        private MemberGetEmitter( Type targetType, Flags bindingFlags, MemberTypes memberType, string fieldOrPropertyName, MemberInfo memberInfo )
            : base(new CallInfo(targetType, null, bindingFlags, memberType, fieldOrPropertyName, Type.EmptyTypes, memberInfo, true))
		{
		}
        internal MemberGetEmitter( CallInfo callInfo ) : base( callInfo )
        {
        }

        protected internal override DynamicMethod CreateDynamicMethod()
        {
            return CreateDynamicMethod("getter", CallInfo.TargetType, Constants.ObjectType, new[] { Constants.ObjectType });
        }

	    protected internal override Delegate CreateDelegate()
		{
	    	MemberInfo member = CallInfo.MemberInfo;
			if( member == null )
			{
		    	member = LookupUtils.GetMember( CallInfo );
				CallInfo.IsStatic = member.IsStatic();
			}
	    	bool handleInnerStruct = CallInfo.ShouldHandleInnerStruct;

			if (handleInnerStruct)
			{
                Generator.ldarg_0                               // load arg-0 (this)
                         .DeclareLocal(CallInfo.TargetType);    // TargetType tmpStr
                LoadInnerStructToLocal(0);                      // tmpStr = ((ValueTypeHolder)this)).Value
				Generator.DeclareLocal(Constants.ObjectType);   // object result;
			}
			else if (!CallInfo.IsStatic)
			{
                Generator.ldarg_0                               // load arg-0 (this)
				         .castclass( CallInfo.TargetType );     // (TargetType)this
			}

			if (member.MemberType == MemberTypes.Field)
			{
				var field = member as FieldInfo;
                Generator.ldfld( field.IsStatic, field )        // (this|tmpStr).field OR TargetType.field
                         .boxIfValueType( field.FieldType );    // (object)<stack>
			}
			else
			{
				var prop = member as PropertyInfo;
                MethodInfo getMethod = LookupUtils.GetPropertyGetMethod(prop, CallInfo);
                Generator.call(getMethod.IsStatic || CallInfo.IsTargetTypeStruct, getMethod ) // (this|tmpStr).prop OR TargetType.prop
                         .boxIfValueType(prop.PropertyType);    // (object)<stack>
			}

			if (handleInnerStruct)
			{
                Generator.stloc_1.end();        // resultLocal = <stack>
				StoreLocalToInnerStruct(0);     // ((ValueTypeHolder)this)).Value = tmpStr
				Generator.ldloc_1.end();        // push resultLocal
			}

	        Generator.ret();

			return Method.CreateDelegate(typeof (MemberGetter));
		}
	}
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Emitter\MemberSetEmitter.cs
//------------------------------------------------------------------------------
#region License

// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/

#endregion


namespace Fasterflect.Emitter
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    internal class MemberSetEmitter : BaseEmitter
    {
        public MemberSetEmitter(MemberInfo memberInfo, Flags bindingFlags)
            : this(memberInfo.DeclaringType, bindingFlags, memberInfo.MemberType, memberInfo.Name, memberInfo)
        {
        }

		public MemberSetEmitter(Type targetType, Flags bindingFlags, MemberTypes memberType, string fieldOrProperty)
            : this(targetType, bindingFlags, memberType, fieldOrProperty, null)
		{
		}

        private MemberSetEmitter(Type targetType, Flags bindingFlags, MemberTypes memberType, string fieldOrProperty, MemberInfo memberInfo)
            : base(new CallInfo(targetType, null, bindingFlags, memberType, fieldOrProperty, Constants.ArrayOfObjectType, memberInfo, false))
        {
        }
        internal MemberSetEmitter(CallInfo callInfo) : base(callInfo)
        {
        }

        protected internal override DynamicMethod CreateDynamicMethod()
        {
            return CreateDynamicMethod("setter", CallInfo.TargetType, null, new[] { Constants.ObjectType, Constants.ObjectType });
        }

		protected internal override Delegate CreateDelegate()
		{
	    	MemberInfo member = CallInfo.MemberInfo;
			if( member == null )
			{
		    	member = LookupUtils.GetMember( CallInfo );
				CallInfo.IsStatic = member.IsStatic();
			}
			bool handleInnerStruct = CallInfo.ShouldHandleInnerStruct;

			if( CallInfo.IsStatic )
			{
				Generator.ldarg_1.end();							// load value-to-be-set
			}
			else 
			{
				Generator.ldarg_0.end();							// load arg-0 (this)
				if (handleInnerStruct)
				{
					Generator.DeclareLocal(CallInfo.TargetType);    // TargetType tmpStr
					LoadInnerStructToLocal(0);                      // tmpStr = ((ValueTypeHolder)this)).Value;
					Generator.ldarg_1.end();                        // load value-to-be-set;
				}
				else
				{
					Generator.castclass( CallInfo.TargetType )      // (TargetType)this
						.ldarg_1.end();								// load value-to-be-set;
				}
			}

            Generator.CastFromObject( member.Type() );				// unbox | cast value-to-be-set
			if (member.MemberType == MemberTypes.Field)
			{
				var field = member as FieldInfo;
                Generator.stfld(field.IsStatic, field);				// (this|tmpStr).field = value-to-be-set;
			}
			else
			{
				var prop = member as PropertyInfo;
				MethodInfo setMethod = LookupUtils.GetPropertySetMethod(prop, CallInfo);
                Generator.call(setMethod.IsStatic || CallInfo.IsTargetTypeStruct, setMethod); // (this|tmpStr).set_Prop(value-to-be-set);
			}

			if (handleInnerStruct)
			{
                StoreLocalToInnerStruct(0); // ((ValueTypeHolder)this)).Value = tmpStr
			}

		    Generator.ret();

			return Method.CreateDelegate(typeof (MemberSetter));
		}
	}
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Emitter\MethodInvocationEmitter.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect.Emitter
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

	internal class MethodInvocationEmitter : InvocationEmitter
	{
		public MethodInvocationEmitter( MethodInfo methodInfo, Flags bindingFlags )
			: this( methodInfo.DeclaringType, bindingFlags, methodInfo.Name, methodInfo.GetParameters().ToTypeArray(), methodInfo )
		{
		}

		public MethodInvocationEmitter( Type targetType, Flags bindingFlags, string name, Type[] parameterTypes )
			: this( targetType, bindingFlags, name, parameterTypes, null )
		{
		}

		private MethodInvocationEmitter( Type targetType, Flags bindingFlags, string name, Type[] parameterTypes,
		                                 MemberInfo methodInfo )
            : base(new CallInfo(targetType, null, bindingFlags, MemberTypes.Method, name, parameterTypes, methodInfo, true))
		{
		}

		public MethodInvocationEmitter( CallInfo callInfo ) : base( callInfo )
		{
		}

		protected internal override DynamicMethod CreateDynamicMethod()
		{
			return CreateDynamicMethod( "invoke", CallInfo.TargetType, Constants.ObjectType,
				new[] { Constants.ObjectType, Constants.ObjectType.MakeArrayType() } );
		}

		protected internal override Delegate CreateDelegate()
		{
			var method = (MethodInfo) CallInfo.MemberInfo ?? LookupUtils.GetMethod( CallInfo );
			CallInfo.IsStatic = method.IsStatic;
			const byte paramArrayIndex = 1;
			bool hasReturnType = method.ReturnType != Constants.VoidType;

			byte startUsableLocalIndex = 0;
			if( CallInfo.HasRefParam )
			{
				startUsableLocalIndex = CreateLocalsForByRefParams( paramArrayIndex, method );
					// create by_ref_locals from argument array
				Generator.DeclareLocal( hasReturnType
				                        	? method.ReturnType
				                        	: Constants.ObjectType ); // T result;
				GenerateInvocation( method, paramArrayIndex, (byte) (startUsableLocalIndex + 1) );
				if( hasReturnType )
				{
					Generator.stloc( startUsableLocalIndex ); // result = <stack>;
				}
				AssignByRefParamsToArray( paramArrayIndex ); // store by_ref_locals back to argument array
			}
			else
			{
				Generator.DeclareLocal( hasReturnType
				                        	? method.ReturnType
				                        	: Constants.ObjectType ); // T result;
				GenerateInvocation( method, paramArrayIndex, (byte) (startUsableLocalIndex + 1) );
				if( hasReturnType )
				{
					Generator.stloc( startUsableLocalIndex ); // result = <stack>;
				}
			}

			if( CallInfo.ShouldHandleInnerStruct )
			{
				StoreLocalToInnerStruct( (byte) (startUsableLocalIndex + 1) ); // ((ValueTypeHolder)this)).Value = tmpStr; 
			}
			if( hasReturnType )
			{
				Generator.ldloc( startUsableLocalIndex ) // push result;
					.boxIfValueType( method.ReturnType ); // box result;
			}
			else
			{
				Generator.ldnull.end(); // load null
			}
			Generator.ret();

			return Method.CreateDelegate( typeof(MethodInvoker) );
		}

		private void GenerateInvocation( MethodInfo methodInfo, byte paramArrayIndex, byte structLocalPosition )
		{
			if( ! CallInfo.IsStatic )
			{
				Generator.ldarg_0.end(); // load arg-0 (this/null);
				if( CallInfo.ShouldHandleInnerStruct )
				{
					Generator.DeclareLocal( CallInfo.TargetType ); // TargetType tmpStr;
					LoadInnerStructToLocal( structLocalPosition ); // tmpStr = ((ValueTypeHolder)this)).Value;
				}
				else
				{
					Generator.castclass( CallInfo.TargetType ); // (TargetType)arg-0;
				}
			}
			PushParamsOrLocalsToStack( paramArrayIndex ); // push arguments and by_ref_locals
			Generator.call( methodInfo.IsStatic || CallInfo.IsTargetTypeStruct, methodInfo ); // call OR callvirt
		}
	}
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Emitter\ValueTypeHolder.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect.Emitter
{
    using System;

    /// <summary>
    /// A wrapper for value type.  Must be used in order for Fasterflect to 
    /// work with value type such as struct.
    /// </summary>
    internal class ValueTypeHolder
    {
        /// <summary>
        /// Creates a wrapper for <paramref name="value"/> value type.  The wrapper
        /// can then be used with Fasterflect.
        /// </summary>
        /// <param name="value">The value type to be wrapped.  
        /// Must be a derivative of <code>ValueType</code>.</param>
        public ValueTypeHolder( object value )
        {
            Value = (ValueType) value;
        }

        /// <summary>
        /// The actual struct wrapped by this instance.
        /// </summary>
        public ValueType Value { get; set; }
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Core\ArrayExtensions.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect
{
    using System;
    using Fasterflect.Emitter;

    /// <summary>
    /// Extension methods for working with arrays.
    /// </summary>
    internal static class ArrayExtensions
    {
        #region Array Access
        /// <summary>
        /// Sets <paramref name="value"/> to the element at position <paramref name="index"/> of <paramref name="array"/>.
        /// </summary>
        /// <returns><paramref name="array"/>.</returns>
        public static object SetElement( this object array, long index, object value )
        {
            ((Array) array).SetValue( value, index );
            return array;
        }

        /// <summary>
        /// Gets the element at position <paramref name="index"/> of <paramref name="array"/>.
        /// </summary>
        public static object GetElement( this object array, long index )
        {
            return ((Array) array).GetValue( index );
        }

        /// <summary>
        /// Creates a delegate which can set element of <paramref name="arrayType"/>.
        /// </summary>
        public static ArrayElementSetter DelegateForSetElement( this Type arrayType )
        {
            return (ArrayElementSetter) new ArraySetEmitter( arrayType ).GetDelegate();
        }

        /// <summary>
        /// Creates a delegate which can retrieve element of <paramref name="arrayType"/>.
        /// </summary>
        public static ArrayElementGetter DelegateForGetElement( this Type arrayType )
        {
            return (ArrayElementGetter) new ArrayGetEmitter( arrayType ).GetDelegate();
        }
        #endregion
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Core\AssemblyExtensions.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

	/// <summary>
	/// Extension methods for inspecting assemblies.
	/// </summary>
	internal static class AssemblyExtensions
	{
		#region Types
		/// <summary>
		/// Gets all types in the given <paramref name="assembly"/> matching the optional list 
		/// <paramref name="names"/>.
		/// </summary>
		/// <param name="assembly">The assembly in which to look for types.</param>
		/// <param name="names">An optional list of names against which to filter the result.  If this is
		/// <c>null</c> or left empty, all types are returned.</param>
		/// <returns>A list of all matching types. This method never returns null.</returns>
		public static IList<Type> Types( this Assembly assembly, params string[] names )
		{
			return assembly.Types( Flags.None, names );
		}

		/// <summary>
		/// Gets all types in the given <paramref name="assembly"/> matching the specified
		/// <paramref name="bindingFlags"/> and the optional list <paramref name="names"/>.
		/// </summary>
		/// <param name="assembly">The assembly in which to look for types.</param>
		/// <param name="bindingFlags">The <see cref="BindingFlags"/> used to customize how results
		/// are filters. If the <see href="Flags.PartialNameMatch"/> option is specified any name
		/// comparisons will use <see href="String.Contains"/> instead of <see href="String.Equals"/>.</param>
		/// <param name="names">An optional list of names against which to filter the result.  If this is
		/// <c>null</c> or left empty, all types are returned.</param>
		/// <returns>A list of all matching types. This method never returns null.</returns>
		public static IList<Type> Types( this Assembly assembly, Flags bindingFlags, params string[] names )
		{
			Type[] types = assembly.GetTypes();

			bool hasNames = names != null && names.Length > 0;
			bool partialNameMatch = bindingFlags.IsSet( Flags.PartialNameMatch );

			return hasNames
			       	? types.Where( t => names.Any( n => partialNameMatch ? t.Name.Contains( n ) : t.Name == n ) ).ToArray()
			       	: types;
		}
		#endregion

		#region TypesImplementing
		/// <summary>
		/// Gets all types in the given <paramref name="assembly"/> that implement the given <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The interface types should implement.</typeparam>
		/// <param name="assembly">The assembly in which to look for types.</param>
		/// <returns>A list of all matching types. This method never returns null.</returns>
		public static IList<Type> TypesImplementing<T>( this Assembly assembly )
		{
			Type[] types = assembly.GetTypes();
			return types.Where( t => t.Implements<T>() ).ToList();
		}
		#endregion

		#region TypesWith Lookup
		/// <summary>
		/// Gets all types in the given <paramref name="assembly"/> that are decorated with an
		/// <see href="Attribute"/> of the given <paramref name="attributeType"/>.
		/// </summary>
		/// <returns>A list of all matching types. This value will never be null.</returns>
		public static IList<Type> TypesWith( this Assembly assembly, Type attributeType )
		{
			IEnumerable<Type> query = from t in assembly.GetTypes()
			                          where t.HasAttribute( attributeType )
			                          select t;
			return query.ToArray();
		}

		/// <summary>
		/// Gets all types in the given <paramref name="assembly"/> that are decorated with an
		/// <see href="Attribute"/> of the given type <typeparamref name="T"/>.
		/// </summary>
		/// <returns>A list of all matching types. This value will never be null.</returns>
		public static IList<Type> TypesWith<T>( this Assembly assembly ) where T : Attribute
		{
			return assembly.TypesWith( typeof(T) );
		}
		#endregion
	}
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Core\AttributeExtensions.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Extension methods for locating and retrieving attributes.
    /// </summary>
    internal static class AttributeExtensions
    {
        #region Attribute Lookup (Single)
        /// <summary>
        /// Gets the first <see href="Attribute"/> associated with the <paramref name="provider"/>.
        /// </summary>
        /// <returns>The first attribute found on the source element.</returns>
        public static Attribute Attribute( this ICustomAttributeProvider provider )
        {
            return provider.Attributes().FirstOrDefault();
        }

        /// <summary>
        /// Gets the first <see href="Attribute"/> of type <paramref name="attributeType"/> associated with the <paramref name="provider"/>.
        /// </summary>
        /// <returns>The first attribute found on the source element.</returns>
        public static Attribute Attribute( this ICustomAttributeProvider provider, Type attributeType )
        {
            return provider.Attributes( attributeType ).FirstOrDefault();
        }

        /// <summary>
        /// Gets the first <see href="Attribute"/> of type <typeparamref name="T"/> associated with the <paramref name="provider"/>.
        /// </summary>
        /// <returns>The first attribute found on the source element.</returns>
        public static T Attribute<T>( this ICustomAttributeProvider provider ) where T : Attribute
        {
            return provider.Attributes<T>().FirstOrDefault();
        }

        /// <summary>
        /// Gets the first <see href="Attribute"/> of type <typeparamref name="T"/> associated with the 
        /// enumeration value given in the <paramref name="provider"/> parameter.
        /// </summary>
        /// <typeparam name="T">The attribute type to search for.</typeparam>
        /// <param name="provider">An enumeration value on which to search for the attribute.</param>
        /// <returns>The first attribute found on the source.</returns>
        public static T Attribute<T>( this Enum provider ) where T : Attribute
        {
            return provider.Attribute( typeof(T) ) as T;
        }

        /// <summary>
        /// Gets the first <see href="Attribute"/> of type <paramref name="attributeType"/> associated with the 
        /// enumeration value given in the <paramref name="provider"/> parameter.
        /// </summary>
        /// <param name="provider">An enumeration value on which to search for the attribute.</param>
        /// <param name="attributeType">The attribute type to search for.</param>
        /// <returns>The first attribute found on the source.</returns>
        public static Attribute Attribute( this Enum provider, Type attributeType )
        {
            Type type = provider.GetType();
            MemberInfo info = type.Member( provider.ToString(), Flags.StaticAnyVisibility | Flags.DeclaredOnly );
            return info.Attribute( attributeType );
        }
        #endregion

        #region Attribute Lookup (Multiple)
        /// <summary>
        /// Gets the <see href="Attribute"/>s associated with the <paramref name="provider"/>. The resulting
        /// list of attributes can optionally be filtered by suppliying a list of <paramref name="attributeTypes"/>
        /// to include.
        /// </summary>
        /// <returns>A list of the attributes found on the source element. This value will never be null.</returns>
        public static IList<Attribute> Attributes( this ICustomAttributeProvider provider, params Type[] attributeTypes )
        {
			bool hasTypes = attributeTypes != null && attributeTypes.Length > 0;
            return provider.GetCustomAttributes( true ).Cast<Attribute>()
				.Where( attr => ! hasTypes || 
					    attributeTypes.Any( at => { Type type = attr.GetType();
													return at == type || at.IsSubclassOf(type); } ) ).ToList();
		}

        /// <summary>
        /// Gets all <see href="Attribute"/>s of type <typeparamref name="T"/> associated with the <paramref name="provider"/>.
        /// </summary>
        /// <returns>A list of the attributes found on the source element. This value will never be null.</returns>
        public static IList<T> Attributes<T>( this ICustomAttributeProvider provider ) where T : Attribute
        {
            return provider.GetCustomAttributes( typeof(T), true ).Cast<T>().ToList();
        }

        /// <summary>
        /// Gets the <see href="Attribute"/>s associated with the enumeration given in <paramref name="provider"/>. 
        /// </summary>
        /// <typeparam name="T">The attribute type to search for.</typeparam>
        /// <param name="provider">An enumeration on which to search for attributes of the given type.</param>
        /// <returns>A list of the attributes found on the supplied source. This value will never be null.</returns>
        public static IList<T> Attributes<T>( this Enum provider ) where T : Attribute
        {
            return provider.Attributes( typeof(T) ).Cast<T>().ToList();
        }

        /// <summary>
        /// Gets the <see href="Attribute"/>s associated with the enumeration given in <paramref name="provider"/>. 
        /// The resulting list of attributes can optionally be filtered by suppliying a list of <paramref name="attributeTypes"/>
        /// to include.
        /// </summary>
        /// <returns>A list of the attributes found on the supplied source. This value will never be null.</returns>
        public static IList<Attribute> Attributes( this Enum provider, params Type[] attributeTypes )
        {
            Type type = provider.GetType();
            MemberInfo info = type.Member( provider.ToString(), Flags.StaticAnyVisibility | Flags.DeclaredOnly );
            return info.Attributes( attributeTypes );
        }
        #endregion

        #region HasAttribute Lookup (Presence Detection)
        /// <summary>
        /// Determines whether the <paramref name="provider"/> element has an associated <see href="Attribute"/>
        /// of type <paramref name="attributeType"/>.
        /// </summary>
        /// <returns>True if the source element has the associated attribute, false otherwise.</returns>
        public static bool HasAttribute( this ICustomAttributeProvider provider, Type attributeType )
        {
            return provider.Attribute( attributeType ) != null;
        }

        /// <summary>
        /// Determines whether the <paramref name="provider"/> element has an associated <see href="Attribute"/>
        /// of type <typeparamref name="T"/>.
        /// </summary>
        /// <returns>True if the source element has the associated attribute, false otherwise.</returns>
        public static bool HasAttribute<T>( this ICustomAttributeProvider provider ) where T : Attribute
        {
            return provider.HasAttribute( typeof(T) );
        }

        /// <summary>
        /// Determines whether the <paramref name="provider"/> element has an associated <see href="Attribute"/>
        /// of any of the types given in <paramref name="attributeTypes"/>.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="attributeTypes">The list of attribute types to look for. If this list is <c>null</c> or
        /// empty an <see href="ArgumentException"/> will be thrown.</param>
        /// <returns>True if the source element has at least one of the specified attribute types, false otherwise.</returns>
        public static bool HasAnyAttribute( this ICustomAttributeProvider provider, params Type[] attributeTypes )
        {
            return provider.Attributes( attributeTypes ).Count() > 0;
        }

        /// <summary>
        /// Determines whether the <paramref name="provider"/> element has an associated <see href="Attribute"/>
        /// of all of the types given in <paramref name="attributeTypes"/>.
        /// </summary>
        /// <returns>True if the source element has all of the specified attribute types, false otherwise.</returns>
        public static bool HasAllAttributes( this ICustomAttributeProvider provider, params Type[] attributeTypes )
        {
			bool hasTypes = attributeTypes != null && attributeTypes.Length > 0;
            return ! hasTypes || attributeTypes.All( at => provider.HasAttribute( at ) );
        }
        #endregion

        #region MembersWith Lookup
        /// <summary>
        /// Gets all public and non-public instance members on the given <paramref name="type"/>.
        /// The resulting list of members can optionally be filtered by supplying a list of 
        /// <paramref name="attributeTypes"/>, in which case only members decorated with at least one of
        /// these will be included.
        /// </summary>
        /// <param name="type">The type on which to reflect.</param>
        /// <param name="memberTypes">The <see href="MemberTypes"/> to include in the search.</param>
        /// <param name="attributeTypes">The optional list of attribute types with which members should
        /// be decorated. If this parameter is <c>null</c> or empty then all fields and properties
        /// will be included in the result.</param>
        /// <returns>A list of all matching members on the type. This value will never be null.</returns>
		public static IList<MemberInfo> MembersWith( this Type type, MemberTypes memberTypes,
                                                     params Type[] attributeTypes )
        {
            return type.MembersWith( memberTypes, Flags.InstanceAnyVisibility, attributeTypes );
        }

        /// <summary>
        /// Gets all members of the given <paramref name="memberTypes"/> on the given <paramref name="type"/> 
        /// that match the specified <paramref name="bindingFlags"/> and are decorated with an
        /// <see href="Attribute"/> of the given type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="type">The type on which to reflect.</param>
        /// <param name="memberTypes">The <see href="MemberTypes"/> to include in the search.</param>
        /// <param name="bindingFlags">The <see cref="BindingFlags"/> or <see cref="Flags"/> combination 
        /// used to define the search behavior and result filtering.</param>
        /// <returns>A list of all matching members on the type. This value will never be null.</returns>
        public static IList<MemberInfo> MembersWith<T>( this Type type, MemberTypes memberTypes, Flags bindingFlags )
        {
            return type.MembersWith( memberTypes, bindingFlags, typeof(T) );
        }

        /// <summary>
        /// Gets all members on the given <paramref name="type"/> that match the specified 
        /// <paramref name="bindingFlags"/>.
        /// The resulting list of members can optionally be filtered by supplying a list of 
        /// <paramref name="attributeTypes"/>, in which case only members decorated with at least one of
        /// these will be included.
        /// </summary>
        /// <param name="type">The type on which to reflect.</param>
        /// <param name="memberTypes">The <see href="MemberTypes"/> to include in the search.</param>
        /// <param name="bindingFlags">The <see cref="BindingFlags"/> or <see cref="Flags"/> combination 
        /// used to define the search behavior and result filtering.</param>
        /// <param name="attributeTypes">The optional list of attribute types with which members should
        /// be decorated. If this parameter is <c>null</c> or empty then all fields and properties
        /// matching the given <paramref name="bindingFlags"/> will be included in the result.</param>
        /// <returns>A list of all matching members on the type. This value will never be null.</returns>
		public static IList<MemberInfo> MembersWith( this Type type, MemberTypes memberTypes, Flags bindingFlags,
                                                     params Type[] attributeTypes )
        {
			bool hasTypes = attributeTypes != null && attributeTypes.Length > 0;
            var query = from m in type.Members( memberTypes, bindingFlags )
                        where ! hasTypes || m.HasAnyAttribute( attributeTypes )
                        select m;
            return query.ToList();
        }

        /// <summary>
        /// Gets all public and non-public instance fields and properties on the given <paramref name="type"/>.
        /// The resulting list of members can optionally be filtered by supplying a list of 
        /// <paramref name="attributeTypes"/>, in which case only members decorated with at least one of
        /// these will be included.
        /// </summary>
        /// <param name="type">The type on which to reflect.</param>
        /// <param name="attributeTypes">The optional list of attribute types with which members should
        /// be decorated. If this parameter is <c>null</c> or empty then all fields and properties
        /// will be included in the result.</param>
        /// <returns>A list of all matching fields and properties on the type. This value will never be null.</returns>
		public static IList<MemberInfo> FieldsAndPropertiesWith( this Type type, params Type[] attributeTypes )
        {
            return type.MembersWith( MemberTypes.Field | MemberTypes.Property, attributeTypes );
        }
		
        /// <summary>
        /// Gets all fields and properties on the given <paramref name="type"/> that match the specified 
        /// <paramref name="bindingFlags"/>.
        /// The resulting list of members can optionally be filtered by supplying a list of 
        /// <paramref name="attributeTypes"/>, in which case only members decorated with at least one of
        /// these will be included.
        /// </summary>
        /// <param name="type">The type on which to reflect.</param>
        /// <param name="bindingFlags">The <see cref="BindingFlags"/> or <see cref="Flags"/> combination 
        /// used to define the search behavior and result filtering.</param>
        /// <param name="attributeTypes">The optional list of attribute types with which members should
        /// be decorated. If this parameter is <c>null</c> or empty then all fields and properties
        /// matching the given <paramref name="bindingFlags"/> will be included in the result.</param>
        /// <returns>A list of all matching fields and properties on the type. This value will never be null.</returns>
		public static IList<MemberInfo> FieldsAndPropertiesWith( this Type type, Flags bindingFlags,
																 params Type[] attributeTypes )
        {
            return type.MembersWith( MemberTypes.Field | MemberTypes.Property, bindingFlags, attributeTypes );
        }
        #endregion

        #region MembersAndAttributes Lookup
        /// <summary>
        /// Gets a dictionary with all public and non-public instance members on the given <paramref name="type"/> 
        /// and their associated attributes. Only members of the given <paramref name="memberTypes"/> will
        /// be included in the result.
        /// The list of attributes associated with each member can optionally be filtered by supplying a list of
        /// <paramref name="attributeTypes"/>, in which case only members with at least one of these will be
        /// included in the result.
        /// </summary>
        /// <returns>An dictionary mapping all matching members to their associated attributes. This value
        /// will never be null. The attribute list associated with each member in the dictionary will likewise
        /// never be null.</returns>
        public static IDictionary<MemberInfo, List<Attribute>> MembersAndAttributes( this Type type,
                                                                                     MemberTypes memberTypes,
                                                                                     params Type[] attributeTypes )
        {
        	return type.MembersAndAttributes( memberTypes, Flags.InstanceAnyVisibility, null );
        }

        /// <summary>
        /// Gets a dictionary with all members on the given <paramref name="type"/> and their associated attributes.
        /// Only members of the given <paramref name="memberTypes"/> and matching <paramref name="bindingFlags"/> will
        /// be included in the result.
        /// The list of attributes associated with each member can optionally be filtered by supplying a list of
        /// <paramref name="attributeTypes"/>, in which case only members with at least one of these will be
        /// included in the result.
        /// </summary>
        /// <returns>An dictionary mapping all matching members to their associated attributes. This value
        /// will never be null. The attribute list associated with each member in the dictionary will likewise
        /// never be null.</returns>
        public static IDictionary<MemberInfo, List<Attribute>> MembersAndAttributes( this Type type,
                                                                                     MemberTypes memberTypes,
                                                                                     Flags bindingFlags,
                                                                                     params Type[] attributeTypes )
        {
            var members = from m in type.Members( memberTypes, bindingFlags )
                          let a = m.Attributes( attributeTypes )
                          where a.Count() > 0
                          select new { Member = m, Attributes = a.ToList() };
            return members.ToDictionary( m => m.Member, m => m.Attributes );
        }
        #endregion
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Core\ConstructorExtensions.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Fasterflect.Emitter;

    /// <summary>
    /// Extension methods for locating, inspecting and invoking constructors.
    /// </summary>
    internal static class ConstructorExtensions
    {
        #region Constructor Invocation (CreateInstance)
        /// <summary>
        /// Invokes a constructor whose parameter types are inferred from <paramref name="parameters" /> 
        /// on the given <paramref name="type"/> with <paramref name="parameters" /> being the arguments.
        /// Leave <paramref name="parameters"/> empty if the constructor has no argument.
        /// </summary>
        /// <remarks>
        /// All elements of <paramref name="parameters"/> must not be <c>null</c>.  Otherwise, 
        /// <see cref="NullReferenceException"/> is thrown.  If you are not sure as to whether
        /// any element is <c>null</c> or not, use the overload that accepts <c>paramTypes</c> array.
        /// </remarks>
        /// <seealso cref="CreateInstance(Type, Type[], object[])"/>
        public static object CreateInstance( this Type type, params object[] parameters )
        {
            return DelegateForCreateInstance( type, parameters.ToTypeArray() )( parameters );
        }

        /// <summary>
        /// Invokes a constructor having parameter types specified by <paramref name="parameterTypes" /> 
        /// on the the given <paramref name="type"/> with <paramref name="parameters" /> being the arguments.
        /// </summary>
        public static object CreateInstance( this Type type, Type[] parameterTypes, params object[] parameters )
        {
            return DelegateForCreateInstance( type, parameterTypes )( parameters );
        }

        /// <summary>
        /// Invokes a constructor whose parameter types are inferred from <paramref name="parameters" /> and
        /// matching <paramref name="bindingFlags"/> on the given <paramref name="type"/> 
        /// with <paramref name="parameters" /> being the arguments. 
        /// Leave <paramref name="parameters"/> empty if the constructor has no argument.
        /// </summary>
        /// <remarks>
        /// All elements of <paramref name="parameters"/> must not be <c>null</c>.  Otherwise, 
        /// <see cref="NullReferenceException"/> is thrown.  If you are not sure as to whether
        /// any element is <c>null</c> or not, use the overload that accepts <c>paramTypes</c> array.
        /// </remarks>
        /// <seealso cref="CreateInstance(System.Type,System.Type[],Fasterflect.Flags,object[])"/>
        public static object CreateInstance( this Type type, Flags bindingFlags, params object[] parameters )
        {
            return DelegateForCreateInstance( type, bindingFlags, parameters.ToTypeArray() )( parameters );
        }

        /// <summary>
        /// Invokes a constructor whose parameter types are <paramref name="parameterTypes" /> and
        /// matching <paramref name="bindingFlags"/> on the given <paramref name="type"/> 
        /// with <paramref name="parameters" /> being the arguments.
        /// </summary>
        public static object CreateInstance( this Type type, Type[] parameterTypes, Flags bindingFlags, params object[] parameters )
        {
            return DelegateForCreateInstance( type, bindingFlags, parameterTypes )( parameters );
        }

        /// <summary>
        /// Creates a delegate which can invoke the constructor whose parameter types are <paramref name="parameterTypes" />
        /// on the given <paramref name="type"/>.  Leave <paramref name="parameterTypes"/> empty if the constructor
        /// has no argument.
        /// </summary>
        public static ConstructorInvoker DelegateForCreateInstance( this Type type, params Type[] parameterTypes )
        {
            return DelegateForCreateInstance( type, Flags.InstanceAnyVisibility, parameterTypes );
        }

        /// <summary>
        /// Creates a delegate which can invoke the constructor whose parameter types are <paramref name="parameterTypes" />
        /// and matching <paramref name="bindingFlags"/> on the given <paramref name="type"/>.  
        /// Leave <paramref name="parameterTypes"/> empty if the constructor has no argument. 
        /// </summary>
        public static ConstructorInvoker DelegateForCreateInstance( this Type type, Flags bindingFlags,
                                                                    params Type[] parameterTypes )
        {
            return (ConstructorInvoker) new CtorInvocationEmitter( type, bindingFlags, parameterTypes ).GetDelegate();
        }
        #endregion

		#region Constructor Invocation (CreateInstances)
		/// <summary>
		/// Finds all types implementing a specific interface or base class <typeparamref name="T"/> in the
		/// given <paramref name="assembly"/> and invokes the default constructor on each to return a list of
		/// instances. Any type that is not a class or does not have a default constructor is ignored.
		/// </summary>
		/// <typeparam name="T">The interface or base class type to look for in the given assembly.</typeparam>
		/// <param name="assembly">The assembly in which to look for types derived from the type parameter.</param>
		/// <returns>A list containing one instance for every unique type implementing T. This will never be null.</returns>
		public static IList<T> CreateInstances<T>( this Assembly assembly )
		{
			var query = from type in assembly.TypesImplementing<T>() 
						where type.IsClass && ! type.IsAbstract && type.Constructor() != null 
						select (T) type.CreateInstance();
			return query.ToList();
		}
    	#endregion

		#region Constructor Lookup (Single)
		/// <summary>
        /// Gets the constructor corresponding to the supplied <paramref name="parameterTypes"/> on the
        /// given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to reflect on.</param>
        /// <param name="parameterTypes">The types of the constructor parameters in order.</param>
        /// <returns>The matching constructor or null if no match was found.</returns>
        public static ConstructorInfo Constructor( this Type type, params Type[] parameterTypes )
        {
            return type.Constructor( Flags.InstanceAnyVisibility, parameterTypes );
        }

        /// <summary>
        /// Gets the constructor matching the given <paramref name="bindingFlags"/> and corresponding to the 
        /// supplied <paramref name="parameterTypes"/> on the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to reflect on.</param>
        /// <param name="bindingFlags">The search criteria to use when reflecting.</param>
        /// <param name="parameterTypes">The types of the constructor parameters in order.</param>
        /// <returns>The matching constructor or null if no match was found.</returns>
        public static ConstructorInfo Constructor( this Type type, Flags bindingFlags, params Type[] parameterTypes )
        {
            return type.GetConstructor( bindingFlags, null, parameterTypes, null );
        }
        #endregion

        #region Constructor Lookup (Multiple)
        /// <summary>
        /// Gets all public and non-public constructors (that are not abstract) on the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to reflect on.</param>
        /// <returns>A list of matching constructors. This value will never be null.</returns>
        public static IList<ConstructorInfo> Constructors( this Type type )
        {
            return type.Constructors( Flags.InstanceAnyVisibility );
        }

        /// <summary>
        /// Gets all constructors matching the given <paramref name="bindingFlags"/> (and that are not abstract)
        /// on the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to reflect on.</param>
        /// <param name="bindingFlags">The search criteria to use when reflecting.</param>
        /// <returns>A list of matching constructors. This value will never be null.</returns>
        public static IList<ConstructorInfo> Constructors( this Type type, Flags bindingFlags )
        {
            return type.GetConstructors( bindingFlags ); //.Where( ci => !ci.IsAbstract ).ToList();
        }
        #endregion
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Core\ConstructorInfoExtensions.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect
{
    using System;
    using System.Reflection;
    using Fasterflect.Emitter;

    /// <summary>
    /// Extension methods for inspecting, invoking and working with constructors.
    /// </summary>
    internal static class ConstructorInfoExtensions
    {
        /// <summary>
        /// Invokes the constructor <paramref name="ctorInfo"/> with <paramref name="parameters"/> as arguments.
        /// Leave <paramref name="parameters"/> empty if the constructor has no argument.
        /// </summary>
        public static object CreateInstance( this ConstructorInfo ctorInfo, params object[] parameters )
        {
            return ctorInfo.DelegateForCreateInstance()( parameters );
        }

        /// <summary>
        /// Creates a delegate which can create instance based on the constructor <paramref name="ctorInfo"/>.
        /// </summary>
        public static ConstructorInvoker DelegateForCreateInstance( this ConstructorInfo ctorInfo )
        {
            return (ConstructorInvoker) new CtorInvocationEmitter( ctorInfo, Flags.InstanceAnyVisibility ).GetDelegate();
        }
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Core\FieldExtensions.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Fasterflect.Emitter;

    /// <summary>
    /// Extension methods for locating and accessing fields.
    /// </summary>
    internal static class FieldExtensions
    {
        #region Field Access
        /// <summary>
        /// Sets the field specified by <paramref name="name"/> on the given <paramref name="obj"/>
        /// to the specified <paramref name="value" />.
        /// </summary>
        /// <returns><paramref name="obj"/>.</returns>
        public static object SetFieldValue( this object obj, string name, object value )
        {
            DelegateForSetFieldValue( obj.GetTypeAdjusted(), name )( obj, value );
            return obj;
        }

        /// <summary>
        /// Gets the value of the field specified by <paramref name="name"/> on the given <paramref name="obj"/>.
        /// </summary>
        public static object GetFieldValue( this object obj, string name )
        {
            return DelegateForGetFieldValue( obj.GetTypeAdjusted(), name )( obj );
        }

        /// <summary>
        /// Sets the field specified by <paramref name="name"/> and matching <paramref name="bindingFlags"/>
        /// on the given <paramref name="obj"/> to the specified <paramref name="value" />.
        /// </summary>
        /// <returns><paramref name="obj"/>.</returns>
        public static object SetFieldValue( this object obj, string name, object value, Flags bindingFlags )
        {
            DelegateForSetFieldValue( obj.GetTypeAdjusted(), name, bindingFlags )( obj, value );
            return obj;
        }

        /// <summary>
        /// Gets the value of the field specified by <paramref name="name"/> and matching <paramref name="bindingFlags"/>
        /// on the given <paramref name="obj"/>.
        /// </summary>
        public static object GetFieldValue( this object obj, string name, Flags bindingFlags )
        {
            return DelegateForGetFieldValue( obj.GetTypeAdjusted(), name, bindingFlags )( obj );
        }

        /// <summary>
        /// Creates a delegate which can set the value of the field specified by <paramref name="name"/> on 
        /// the given <paramref name="type"/>.
        /// </summary>
        public static MemberSetter DelegateForSetFieldValue( this Type type, string name )
        {
            return DelegateForSetFieldValue( type, name, Flags.StaticInstanceAnyVisibility );
        }

        /// <summary>
        /// Creates a delegate which can get the value of the field specified by <paramref name="name"/> on 
        /// the given <paramref name="type"/>.
        /// </summary>
        public static MemberGetter DelegateForGetFieldValue( this Type type, string name )
        {
            return DelegateForGetFieldValue( type, name, Flags.StaticInstanceAnyVisibility );
        }

        /// <summary>
        /// Creates a delegate which can set the value of the field specified by <paramref name="name"/> and
        /// matching <paramref name="bindingFlags"/> on the given <paramref name="type"/>.
        /// </summary>
        public static MemberSetter DelegateForSetFieldValue( this Type type, string name, Flags bindingFlags )
        {
            var callInfo = new CallInfo(type, null, bindingFlags, MemberTypes.Field, name, null, null, false);
			return (MemberSetter) new MemberSetEmitter( callInfo ).GetDelegate();
        }

        /// <summary>
        /// Creates a delegate which can get the value of the field specified by <paramref name="name"/> and
        /// matching <paramref name="bindingFlags"/> on the given <paramref name="type"/>.
        /// </summary>
        public static MemberGetter DelegateForGetFieldValue( this Type type, string name, Flags bindingFlags )
        {
            var callInfo = new CallInfo(type, null, bindingFlags, MemberTypes.Field, name, null, null, true);
			return (MemberGetter) new MemberGetEmitter( callInfo ).GetDelegate();
        }
        #endregion

        #region Field Lookup (Single)
        /// <summary>
        /// Gets the field identified by <paramref name="name"/> on the given <paramref name="type"/>. This method 
        /// searches for public and non-public instance fields on both the type itself and all parent classes.
        /// </summary>
        /// <returns>A single FieldInfo instance of the first found match or null if no match was found.</returns>
        public static FieldInfo Field( this Type type, string name )
        {
            return type.Field( name, Flags.InstanceAnyVisibility );
        }

        /// <summary>
        /// Gets the field identified by <paramref name="name"/> on the given <paramref name="type"/>. 
        /// Use the <paramref name="bindingFlags"/> parameter to define the scope of the search.
        /// </summary>
        /// <returns>A single FieldInfo instance of the first found match or null if no match was found.</returns>
        public static FieldInfo Field( this Type type, string name, Flags bindingFlags )
        {
            // we need to check all fields to do partial name matches
            if( bindingFlags.IsAnySet( Flags.PartialNameMatch | Flags.TrimExplicitlyImplemented ) )
            {
                return type.Fields( bindingFlags, name ).FirstOrDefault();
            }

            var result = type.GetField( name, bindingFlags );
            if( result == null && bindingFlags.IsNotSet( Flags.DeclaredOnly ) )
            {
                if( type.BaseType != typeof(object) && type.BaseType != null )
                {
                    return type.BaseType.Field( name, bindingFlags );
                }
            }
            bool hasSpecialFlags =
                bindingFlags.IsAnySet( Flags.ExcludeBackingMembers | Flags.ExcludeExplicitlyImplemented );
            if( hasSpecialFlags )
            {
                IList<FieldInfo> fields = new List<FieldInfo> { result };
                fields = fields.Filter( bindingFlags );
                return fields.Count > 0 ? fields[ 0 ] : null;
            }
            return result;
        }
        #endregion

        #region Field Lookup (Multiple)
        /// <summary>
        /// Gets all public and non-public instance fields on the given <paramref name="type"/>,
        /// including fields defined on base types.
        /// </summary>
        /// <param name="type">The type on which to reflect.</param>
        /// <param name="names">The optional list of names against which to filter the result. If this parameter is
		/// <c>null</c> or empty no name filtering will be applied. This method will check for an exact, 
		/// case-sensitive match.</param>
        /// <returns>A list of all instance fields on the type. This value will never be null.</returns>
        public static IList<FieldInfo> Fields( this Type type, params string[] names )
        {
            return type.Fields( Flags.InstanceAnyVisibility, names );
        }

        /// <summary>
        /// Gets all fields on the given <paramref name="type"/> that match the specified <paramref name="bindingFlags"/>.
        /// </summary>
        /// <param name="type">The type on which to reflect.</param>
        /// <param name="bindingFlags">The <see cref="BindingFlags"/> or <see cref="Flags"/> combination used to define
        /// the search behavior and result filtering.</param>
        /// <param name="names">The optional list of names against which to filter the result. If this parameter is
		/// <c>null</c> or empty no name filtering will be applied. The default behavior is to check for an exact, 
		/// case-sensitive match. Pass <see href="Flags.ExcludeExplicitlyImplemented"/> to exclude explicitly implemented 
		/// interface members, <see href="Flags.PartialNameMatch"/> to locate by substring, and 
		/// <see href="Flags.IgnoreCase"/> to ignore case.</param>
        /// <returns>A list of all matching fields on the type. This value will never be null.</returns>
        public static IList<FieldInfo> Fields( this Type type, Flags bindingFlags, params string[] names )
        {
            if( type == null || type == typeof(object) )
            {
                return new FieldInfo[0];
            }

            bool recurse = bindingFlags.IsNotSet( Flags.DeclaredOnly );
            bool hasNames = names != null && names.Length > 0;
            bool hasSpecialFlags =
                bindingFlags.IsAnySet( Flags.ExcludeBackingMembers | Flags.ExcludeExplicitlyImplemented );

            if( ! recurse && ! hasNames && ! hasSpecialFlags )
            {
                return type.GetFields( bindingFlags ) ?? new FieldInfo[0];
            }

            var fields = GetFields( type, bindingFlags );
            fields = hasSpecialFlags ? fields.Filter( bindingFlags ) : fields;
            fields = hasNames ? fields.Filter( bindingFlags, names ) : fields;
            return fields;
        }

        private static IList<FieldInfo> GetFields( Type type, Flags bindingFlags )
        {
            bool recurse = bindingFlags.IsNotSet( Flags.DeclaredOnly );

            if( ! recurse )
            {
                return type.GetFields( bindingFlags ) ?? new FieldInfo[0];
            }

            bindingFlags |= Flags.DeclaredOnly;
            bindingFlags &= ~BindingFlags.FlattenHierarchy;

            var fields = new List<FieldInfo>();
            fields.AddRange( type.GetFields( bindingFlags ) );
            Type baseType = type.BaseType;
            while( baseType != null && baseType != typeof(object) )
            {
                fields.AddRange( baseType.GetFields( bindingFlags ) );
                baseType = baseType.BaseType;
            }
            return fields;
        }
        #endregion

        #region Field Combined

        #region TryGetValue
		/// <summary>
        /// Gets the first (public or non-public) instance field with the given <paramref name="name"/> on the given
        /// <paramref name="obj"/> object. Returns the value of the field if a match was found and null otherwise.
		/// </summary>
		/// <remarks>
        /// When using this method it is not possible to distinguish between a missing field and a field whose value is null.
		/// </remarks>
		/// <param name="obj">The source object on which to find the field</param>
		/// <param name="name">The name of the field whose value should be retrieved</param>
		/// <returns>The value of the field or null if no field was found</returns>
		public static object TryGetFieldValue( this object obj, string name )
        {
            return TryGetFieldValue( obj, name, Flags.InstanceAnyVisibility );
        }

		/// <summary>
        /// Gets the first field with the given <paramref name="name"/> on the given <paramref name="obj"/> object.
        /// Returns the value of the field if a match was found and null otherwise.
        /// Use the <paramref name="bindingFlags"/> parameter to limit the scope of the search.
		/// </summary>
		/// <remarks>
        /// When using this method it is not possible to distinguish between a missing field and a field whose value is null.
		/// </remarks>
		/// <param name="obj">The source object on which to find the field</param>
		/// <param name="name">The name of the field whose value should be retrieved</param>
		/// <param name="bindingFlags">A combination of Flags that define the scope of the search</param>
		/// <returns>The value of the field or null if no field was found</returns>
        public static object TryGetFieldValue( this object obj, string name, Flags bindingFlags )
        {
            try
            {
                return obj.GetFieldValue( name, bindingFlags );
            }
            catch( MissingFieldException )
            {
                return null;
            }
        }
        #endregion

        #region TrySetValue
		/// <summary>
        /// Sets the first (public or non-public) instance field with the given <paramref name="name"/> on the 
        /// given <paramref name="obj"/> object to supplied <paramref name="value"/>. Returns true if a value
        /// was assigned to a field and false otherwise.
		/// </summary>
		/// <param name="obj">The source object on which to find the field</param>
		/// <param name="name">The name of the field whose value should be retrieved</param>
		/// <param name="value">The value that should be assigned to the field</param>
		/// <returns>True if the value was assigned to a field and false otherwise</returns>
        public static bool TrySetFieldValue( this object obj, string name, object value )
        {
            return TrySetFieldValue( obj, name, value, Flags.InstanceAnyVisibility );
        }

		/// <summary>
        /// Sets the first field with the given <paramref name="name"/> on the given <paramref name="obj"/> object
        /// to the supplied <paramref name="value"/>. Returns true if a value was assigned to a field and false otherwise.
        /// Use the <paramref name="bindingFlags"/> parameter to limit the scope of the search.
		/// </summary>
		/// <param name="obj">The source object on which to find the field</param>
		/// <param name="name">The name of the field whose value should be retrieved</param>
		/// <param name="value">The value that should be assigned to the field</param>
		/// <param name="bindingFlags">A combination of Flags that define the scope of the search</param>
		/// <returns>True if the value was assigned to a field and false otherwise</returns>
        public static bool TrySetFieldValue( this object obj, string name, object value, Flags bindingFlags )
        {
            try
            {
                obj.SetFieldValue(name, value, bindingFlags );
                return true;
            }
            catch( MissingFieldException )
            {
                return false;
            }
        }
        #endregion

        #endregion
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Core\FieldInfoExtensions.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect
{
    using System.Reflection;
    using Fasterflect.Emitter;

	/// <summary>
    /// Extension methods for inspecting and working with fields.
    /// </summary>
    internal static class FieldInfoExtensions
    {
        /// <summary>
        /// Sets the static field identified by <paramref name="fieldInfo"/> to the specified <paramref name="value" />.
        /// </summary>
        public static void Set( this FieldInfo fieldInfo, object value )
        {
            fieldInfo.DelegateForSetFieldValue()( null, value );
        }

        /// <summary>
        /// Sets the instance field identified by <paramref name="fieldInfo"/> on the given <paramref name="obj"/>
        /// to the specified <paramref name="value" />.
        /// </summary>
        public static void Set( this FieldInfo fieldInfo, object obj, object value )
        {
            fieldInfo.DelegateForSetFieldValue()( obj, value );
        }

        /// <summary>
        /// Gets the value of the static field identified by <paramref name="fieldInfo"/>.
        /// </summary>
        public static object Get( this FieldInfo fieldInfo )
        {
            return fieldInfo.DelegateForGetFieldValue()( null );
        }

        /// <summary>
        /// Gets the value of the instance field identified by <paramref name="fieldInfo"/> on the given <paramref name="obj"/>.
        /// </summary>
        public static object Get( this FieldInfo fieldInfo, object obj )
        {
            return fieldInfo.DelegateForGetFieldValue()( obj );
        }

        /// <summary>
        /// Creates a delegate which can set the value of the field identified by <paramref name="fieldInfo"/>.
        /// </summary>
        public static MemberSetter DelegateForSetFieldValue( this FieldInfo fieldInfo )
        {
        	var flags = fieldInfo.IsStatic ? Flags.StaticAnyVisibility : Flags.InstanceAnyVisibility;
            return (MemberSetter) new MemberSetEmitter( fieldInfo, flags ).GetDelegate();
        }

        /// <summary>
        /// Creates a delegate which can get the value of the field identified by <paramref name="fieldInfo"/>.
        /// </summary>
        public static MemberGetter DelegateForGetFieldValue( this FieldInfo fieldInfo )
        {
        	var flags = fieldInfo.IsStatic ? Flags.StaticAnyVisibility : Flags.InstanceAnyVisibility;
            return (MemberGetter) new MemberGetEmitter( fieldInfo, flags ).GetDelegate();
        }
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Core\MemberExtensions.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Extension methods for locating and accessing fields or properties, for situations where
    /// you do not care which it is.
    /// </summary>
    internal static class MemberExtensions
    {
        #region Member Lookup (Single)
        /// <summary>
        /// Gets the member identified by <paramref name="name"/> on the given <paramref name="type"/>. This 
        /// method searches for public and non-public instance fields on both the type itself and all parent classes.
        /// </summary>
        /// <returns>A single MemberInfo instance of the first found match or null if no match was found.</returns>
        public static MemberInfo Member( this Type type, string name )
        {
            return type.Members( MemberTypes.All, Flags.InstanceAnyVisibility, name ).FirstOrDefault();
        }

        /// <summary>
        /// Gets the member identified by <paramref name="name"/> on the given <paramref name="type"/>. Use 
        /// the <paramref name="bindingFlags"/> parameter to define the scope of the search.
        /// </summary>
        /// <returns>A single MemberInfo instance of the first found match or null if no match was found.</returns>
        public static MemberInfo Member( this Type type, string name, Flags bindingFlags )
        {
            // we need to check all members to do partial name matches
            if( bindingFlags.IsAnySet( Flags.PartialNameMatch | Flags.TrimExplicitlyImplemented ) )
            {
                return type.Members( MemberTypes.All, bindingFlags, name ).FirstOrDefault();
            }

            IList<MemberInfo> result = type.GetMember( name, bindingFlags );
            bool hasSpecialFlags =
                bindingFlags.IsAnySet( Flags.ExcludeBackingMembers | Flags.ExcludeExplicitlyImplemented );
            result = hasSpecialFlags && result.Count > 0 ? result.Filter( bindingFlags ) : result;
            bool found = result.Count > 0;

            if( !found && bindingFlags.IsNotSet( Flags.DeclaredOnly ) )
            {
                if( type.BaseType != typeof(object) && type.BaseType != null )
                {
                    return type.BaseType.Member( name, bindingFlags );
                }
            }
            return found ? result[ 0 ] : null;
        }
        #endregion

        #region Member Lookup (FieldsAndProperties)
        /// <summary>
        /// Gets all public and non-public instance fields and properties on the given <paramref name="type"/>, 
        /// including members defined on base types.
        /// </summary>
        /// <returns>A list of all matching members on the type. This value will never be null.</returns>
        public static IList<MemberInfo> FieldsAndProperties( this Type type )
        {
            return type.Members( MemberTypes.Field | MemberTypes.Property, Flags.InstanceAnyVisibility, null );
        }

        /// <summary>
        /// Gets all public and non-public instance fields and properties on the given <paramref name="type"/> 
        /// that match the specified <paramref name="bindingFlags"/>, including members defined on base types.
        /// </summary>
        /// <returns>A list of all matching members on the type. This value will never be null.</returns>
        public static IList<MemberInfo> FieldsAndProperties( this Type type, Flags bindingFlags )
        {
            return type.Members( MemberTypes.Field | MemberTypes.Property, bindingFlags, null );
        }
        #endregion

        #region Member Lookup (Multiple)
        /// <summary>
        /// Gets all public and non-public instance members on the given <paramref name="type"/>.
        /// </summary>
        /// <returns>A list of all members on the type. This value will never be null.</returns>
		/// <param name="type">The type to reflect on.</param>
        /// <returns>A list of all members on the type. This value will never be null.</returns>
        public static IList<MemberInfo> Members( this Type type )
        {
            return type.Members( MemberTypes.All, Flags.InstanceAnyVisibility, null );
        }

        /// <summary>
        /// Gets all public and non-public instance members on the given <paramref name="type"/> that 
        /// match the specified <paramref name="bindingFlags"/>.
        /// </summary>
        /// <returns>A list of all matching members on the type. This value will never be null.</returns>
		/// <param name="type">The type to reflect on.</param>
        /// <param name="bindingFlags">The <see cref="BindingFlags"/> or <see cref="Flags"/> combination used to define
        /// the search behavior and result filtering.</param>
        /// <returns>A list of all matching members on the type. This value will never be null.</returns>
        public static IList<MemberInfo> Members( this Type type, Flags bindingFlags )
        {
            return type.Members( MemberTypes.All, bindingFlags, null );
        }

        /// <summary>
        /// Gets all public and non-public instance members of the given <paramref name="memberTypes"/> on the 
        /// given <paramref name="type"/>, optionally filtered by the supplied <paramref name="names"/> list.
        /// </summary>
		/// <param name="memberTypes">The <see href="MemberTypes"/> to include in the result.</param>
        /// <param name="type">The type on which to reflect.</param>
        /// <param name="names">The optional list of names against which to filter the result. If this parameter is
		/// <c>null</c> or empty no name filtering will be applied. The default behavior is to check for an exact, 
		/// case-sensitive match. Pass <see href="Flags.ExcludeExplicitlyImplemented"/> to exclude explicitly implemented 
		/// interface members, <see href="Flags.PartialNameMatch"/> to locate by substring, and 
		/// <see href="Flags.IgnoreCase"/> to ignore case.</param>
        /// <returns>A list of all matching members on the type. This value will never be null.</returns>
        public static IList<MemberInfo> Members( this Type type, MemberTypes memberTypes, params string[] names )
        {
        	return type.Members( memberTypes, Flags.InstanceAnyVisibility, names );
        }

    	/// <summary>
        /// Gets all members of the given <paramref name="memberTypes"/> on the given <paramref name="type"/> that 
        /// match the specified <paramref name="bindingFlags"/>, optionally filtered by the supplied <paramref name="names"/>
        /// list (in accordance with the given <paramref name="bindingFlags"/>).
        /// </summary>
		/// <param name="type">The type to reflect on.</param>
		/// <param name="memberTypes">The <see href="MemberTypes"/> to include in the result.</param>
        /// <param name="bindingFlags">The <see cref="BindingFlags"/> or <see cref="Flags"/> combination used to define
        /// the search behavior and result filtering.</param>
        /// <param name="names">The optional list of names against which to filter the result. If this parameter is
		/// <c>null</c> or empty no name filtering will be applied. The default behavior is to check for an exact, 
		/// case-sensitive match. Pass <see href="Flags.ExcludeExplicitlyImplemented"/> to exclude explicitly implemented 
		/// interface members, <see href="Flags.PartialNameMatch"/> to locate by substring, and 
		/// <see href="Flags.IgnoreCase"/> to ignore case.</param>
        /// <returns>A list of all matching members on the type. This value will never be null.</returns>
        public static IList<MemberInfo> Members( this Type type, MemberTypes memberTypes, Flags bindingFlags,
                                                 params string[] names )
        {
            if( type == null || type == typeof(object) )
            {
                return new MemberInfo[0];
            }

            bool recurse = bindingFlags.IsNotSet( Flags.DeclaredOnly );
            bool hasNames = names != null && names.Length > 0;
            bool hasSpecialFlags =
                bindingFlags.IsAnySet( Flags.ExcludeBackingMembers | Flags.ExcludeExplicitlyImplemented );

            if( ! recurse && ! hasNames && ! hasSpecialFlags )
            {
                return type.FindMembers( memberTypes, bindingFlags, null, null );
            }

            var members = GetMembers( type, memberTypes, bindingFlags );
            members = hasSpecialFlags ? members.Filter( bindingFlags ) : members;
            members = hasNames ? members.Filter( bindingFlags, names ) : members;
            return members;
        }

        private static IList<MemberInfo> GetMembers( Type type, MemberTypes memberTypes, Flags bindingFlags )
        {
            bool recurse = bindingFlags.IsNotSet( Flags.DeclaredOnly );

            if( ! recurse )
            {
                return type.FindMembers( memberTypes, bindingFlags, null, null );
            }

            bindingFlags |= Flags.DeclaredOnly;
            bindingFlags &= ~BindingFlags.FlattenHierarchy;

            var members = new List<MemberInfo>();
            members.AddRange( type.FindMembers( memberTypes, bindingFlags, null, null ) );
            Type baseType = type.BaseType;
            while( baseType != null && baseType != typeof(object) )
            {
                members.AddRange( baseType.FindMembers( memberTypes, bindingFlags, null, null ) );
                baseType = baseType.BaseType;
            }
            return members;
        }
        #endregion

        #region Member Combined

        #region TryGetValue
		/// <summary>
        /// Gets the first (public or non-public) instance member with the given <paramref name="name"/> on the given
        /// <paramref name="obj"/> object. Returns the value of the member if a match was found and null otherwise.
		/// </summary>
		/// <remarks>
        /// When using this method it is not possible to distinguish between a missing member and a member whose value is null.
		/// </remarks>
		/// <param name="obj">The source object on which to find the member</param>
		/// <param name="name">The name of the member whose value should be retrieved</param>
		/// <returns>The value of the member or null if no member was found</returns>
        public static object TryGetValue( this object obj, string name )
        {
            return TryGetValue( obj, name, Flags.InstanceAnyVisibility );
        }

		/// <summary>
        /// Gets the first member with the given <paramref name="name"/> on the given <paramref name="obj"/> object.
        /// Returns the value of the member if a match was found and null otherwise.
        /// Use the <paramref name="bindingFlags"/> parameter to limit the scope of the search.
		/// </summary>
		/// <remarks>
        /// When using this method it is not possible to distinguish between a missing member and a member whose value is null.
		/// </remarks>
		/// <param name="obj">The source object on which to find the member</param>
		/// <param name="name">The name of the member whose value should be retrieved</param>
		/// <param name="bindingFlags">A combination of Flags that define the scope of the search</param>
		/// <returns>The value of the member or null if no member was found</returns>
        public static object TryGetValue( this object obj, string name, Flags bindingFlags )
		{
			Type type = obj.GetType();
			var info = type.Member( name, bindingFlags );
			if( info == null )
			{
				return null;
			}
			bool valid = info is FieldInfo || info is PropertyInfo;
			return valid ? info.Get( obj ) : null;
        }
        #endregion

        #region TrySetValue
		/// <summary>
        /// Sets the first (public or non-public) instance member with the given <paramref name="name"/> on the 
        /// given <paramref name="obj"/> object to the supplied <paramref name="value"/>. Returns true 
        /// if a value was assigned to a member and false otherwise.
		/// </summary>
		/// <param name="obj">The source object on which to find the member</param>
		/// <param name="name">The name of the member whose value should be retrieved</param>
		/// <param name="value">The value that should be assigned to the member</param>
		/// <returns>True if the value was assigned to a member and false otherwise</returns>
        public static bool TrySetValue( this object obj, string name, object value )
        {
            return TrySetValue( obj, name, value, Flags.InstanceAnyVisibility );
        }

		/// <summary>
        /// Sets the first member with the given <paramref name="name"/> on the given <paramref name="obj"/> object
        /// to the supplied <paramref name="value"/>. Returns true if a value was assigned to a member and false otherwise.
        /// Use the <paramref name="bindingFlags"/> parameter to limit the scope of the search.
		/// </summary>
		/// <param name="obj">The source object on which to find the member</param>
		/// <param name="name">The name of the member whose value should be retrieved</param>
		/// <param name="value">The value that should be assigned to the member</param>
		/// <param name="bindingFlags">A combination of Flags that define the scope of the search</param>
		/// <returns>True if the value was assigned to a member and false otherwise</returns>
        public static bool TrySetValue( this object obj, string name, object value, Flags bindingFlags )
        {
            Type type = obj.GetType();
            var property = type.Property( name, bindingFlags );
            if( property != null && property.CanWrite )
            {
                property.Set( obj, value );
                return true;
            }
            var field = type.Field( name, bindingFlags );
            if( field != null )
            {
                field.Set( obj, value );
                return true;
            }
            return false;
        }
        #endregion

        #endregion
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Core\MemberInfoExtensions.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect
{
    using System;
    using System.Reflection;
    using Fasterflect.Emitter;

	/// <summary>
    /// Extension methods for inspecting and working with members.
    /// </summary>
    internal static class MemberInfoExtensions
    {
        /// <summary>
        /// Gets the static field or property identified by <paramref name="memberInfo"/>.
        /// </summary>
        public static object Get( this MemberInfo memberInfo )
        {
            var @delegate = (MemberGetter) new MemberGetEmitter( memberInfo, Flags.StaticAnyVisibility ).GetDelegate();
            return @delegate( null );
        }

        /// <summary>
        /// Sets the static field or property identified by <paramref name="memberInfo"/> with <paramref name="value"/>.
        /// </summary>
        public static void Set( this MemberInfo memberInfo, object value )
        {
            var @delegate = (MemberSetter) new MemberSetEmitter( memberInfo, Flags.StaticAnyVisibility ).GetDelegate();
            @delegate( null, value );
        }

        /// <summary>
        /// Gets the instance field or property identified by <paramref name="memberInfo"/> on
        /// the <paramref name="obj"/>.
        /// </summary>
        public static object Get( this MemberInfo memberInfo, object obj )
        {
            var @delegate = (MemberGetter) new MemberGetEmitter( memberInfo, Flags.InstanceAnyVisibility ).GetDelegate();
            return @delegate( obj );
        }

        /// <summary>
        /// Sets the instance field or property identified by <paramref name="memberInfo"/> on
        /// the <paramref name="obj"/> object with <paramref name="value"/>.
        /// </summary>
        public static void Set( this MemberInfo memberInfo, object obj, object value )
        {
            var @delegate = (MemberSetter) new MemberSetEmitter( memberInfo, Flags.InstanceAnyVisibility ).GetDelegate();
            @delegate( obj, value );
        }

        #region MemberInfo Helpers
        /// <summary>
        /// Gets the system type of the field or property identified by the <paramref name="member"/>.
        /// </summary>
        /// <returns>The system type of the member.</returns>
        public static Type Type( this MemberInfo member )
        {
            var field = member as FieldInfo;
            if( field != null )
            {
                return field.FieldType;
            }
            var property = member as PropertyInfo;
            if( property != null )
            {
                return property.PropertyType;
            }
            throw new NotSupportedException( "Can only determine the type for fields and properties." );
        }

        /// <summary>
        /// Determines whether a value can be read from the field or property identified by
        /// the <paramref name="member"/>.
        /// </summary>
        /// <returns>True for fields and readable properties, false otherwise.</returns>
        public static bool IsReadable( this MemberInfo member )
        {
            var property = member as PropertyInfo;
            return member is FieldInfo || (property != null && property.CanRead);
        }

        /// <summary>
        /// Determines whether a value can be assigned to the field or property identified by
        /// the <paramref name="member"/>.
        /// </summary>
        /// <returns>True for updateable fields and properties, false otherwise.</returns>
        public static bool IsWritable( this MemberInfo member )
        {
        	var field = member as FieldInfo;
            var property = member as PropertyInfo;
            return (field != null && ! field.IsInitOnly && ! field.IsLiteral) || (property != null && property.CanWrite);
        }
        /// <summary>
        /// Determines whether the given <paramref name="member"/> is invokable.
        /// </summary>
        /// <returns>True for methods and constructors, false otherwise.</returns>
        public static bool IsInvokable( this MemberInfo member )
        {
        	return member is MethodBase;
        }

        /// <summary>
        /// Determines whether the given <paramref name="member"/> is a static member.
        /// </summary>
        /// <returns>True for static fields, properties and methods and false for instance fields,
        /// properties and methods. Throws an exception for all other <see href="MemberTypes" />.</returns>
        public static bool IsStatic( this MemberInfo member )
        {
			var field = member as FieldInfo;
            if( field != null )
            	return field.IsStatic;
			var property = member as PropertyInfo;
            if( property != null )
            	return property.CanRead ? property.GetGetMethod( true ).IsStatic : property.GetSetMethod( true ).IsStatic;
			var method = member as MethodInfo;
            if( method != null )
            	return method.IsStatic;
			string message = string.Format( "Unable to determine IsStatic for member {0}.{1}"+
				"MemberType was {2} but only fields, properties and methods are supported.", 
				member.Name, member.MemberType, Environment.NewLine );
        	throw new NotSupportedException( message );
		}

        /// <summary>
        /// Determines whether the given <paramref name="member"/> is an instance member.
        /// </summary>
        /// <returns>True for instance fields, properties and methods and false for static fields,
        /// properties and methods. Throws an exception for all other <see href="MemberTypes" />.</returns>
        public static bool IsInstance( this MemberInfo member )
        {
        	return ! member.IsStatic();
		}

        /// <summary>
        /// Determines whether the given <paramref name="member"/> has the given <paramref name="name"/>.
        /// The comparison uses OrdinalIgnoreCase and allows for a leading underscore in either name
        /// to be ignored.
        /// </summary>
        /// <returns>True if the name is considered identical, false otherwise. If either parameter
        /// is null an exception will be thrown.</returns>
        public static bool HasName( this MemberInfo member, string name )
        {
            string memberName = member.Name.Length > 0 && member.Name[ 0 ] == '_'
                                    ? member.Name.Substring( 1 )
                                    : member.Name;
            name = name.Length > 0 && name[ 0 ] == '_' ? name.Substring( 1 ) : name;
            return memberName.Equals( name, StringComparison.OrdinalIgnoreCase );
        }
        #endregion
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Core\MethodExtensions.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Fasterflect.Emitter;

    /// <summary>
    /// Extension methods for locating, inspecting and invoking methods.
    /// </summary>
    internal static class MethodExtensions
    {
        #region Method Invocation
        /// <summary>
        /// Invokes the method specified by <paramref name="name"/> on the given <paramref name="obj"/> 
        /// using <paramref name="parameters"/> as arguments. 
        /// Leave <paramref name="parameters"/> empty if the method has no arguments.
        /// </summary>
        /// <returns>The return value of the method.</returns>
        /// <remarks>If the method has no return type, <c>null</c> is returned.</remarks>
        /// <remarks>
        /// All elements of <paramref name="parameters"/> must not be <c>null</c>.  Otherwise, 
        /// <see cref="NullReferenceException"/> is thrown.  If you are not sure as to whether
        /// any element is <c>null</c> or not, use the overload that accepts <c>paramTypes</c> array.
        /// </remarks>
        /// <seealso cref="CallMethod(object,string,System.Type[],object[])"/>
        public static object CallMethod( this object obj, string name, params object[] parameters )
        {
            return DelegateForCallMethod( obj.GetTypeAdjusted(), name, parameters.ToTypeArray() )( obj, parameters );
        }

        /// <summary>
        /// Invoke generic method.  See the overload with same parameters except for <paramref name="genericTypes"/>.
        /// </summary>
        /// <seealso cref="CallMethod(object,string,object[])"/>
        public static object CallMethod(this object obj, Type[] genericTypes, string name, params object[] parameters)
        {
            return DelegateForCallMethod(obj.GetTypeAdjusted(), genericTypes, name, parameters.ToTypeArray())(obj, parameters);
        }


        /// <summary>
        /// Invokes the method specified by <paramref name="name"/> on the given <paramref name="obj"/> 
        /// using <paramref name="parameters"/> as arguments.
        /// Method parameter types are specified by <paramref name="parameterTypes"/>.
        /// </summary>
        /// <returns>The return value of the method.</returns>
        /// <remarks>If the method has no return type, <c>null</c> is returned.</remarks>
        public static object CallMethod( this object obj, string name, Type[] parameterTypes, params object[] parameters )
        {
            return DelegateForCallMethod( obj.GetTypeAdjusted(), name, parameterTypes )( obj, parameters );
        }

        /// <summary>
        /// Invoke generic method.  See the overload with same parameters except for <paramref name="genericTypes"/>.
        /// </summary>
        /// <seealso cref="CallMethod(object,string,Type[],object[])"/>
        public static object CallMethod(this object obj, Type[] genericTypes, string name, Type[] parameterTypes, params object[] parameters)
        {
            return DelegateForCallMethod(obj.GetTypeAdjusted(), genericTypes, name, parameterTypes)(obj, parameters);
        }

        /// <summary>
        /// Invokes the method specified by <paramref name="name"/> on the given <paramref name="obj"/>
        /// matching <paramref name="bindingFlags"/> using <paramref name="parameters"/> as arguments.
        /// Leave <paramref name="parameters"/> empty if the method has no argument.
        /// </summary>
        /// <returns>The return value of the method.</returns>
        /// <remarks>If the method has no return type, <c>null</c> is returned.</remarks>
        /// <remarks>
        /// All elements of <paramref name="parameters"/> must not be <c>null</c>.  Otherwise, 
        /// <see cref="NullReferenceException"/> is thrown.  If you are not sure as to whether
        /// any element is <c>null</c> or not, use the overload that accepts <c>paramTypes</c> array.
        /// </remarks>
        /// <seealso cref="CallMethod(object,string,System.Type[],Fasterflect.Flags,object[])"/>
        public static object CallMethod( this object obj, string name, Flags bindingFlags, params object[] parameters )
        {
            return DelegateForCallMethod( obj.GetTypeAdjusted(), name, bindingFlags, parameters.ToTypeArray() )( obj, parameters );
        }

        /// <summary>
        /// Invoke generic method.  See the overload with same parameters except for <paramref name="genericTypes"/>.
        /// </summary>
        /// <seealso cref="CallMethod(object,string,Flags,object[])"/>
        public static object CallMethod(this object obj, Type[] genericTypes, string name, Flags bindingFlags, params object[] parameters)
        {
            return DelegateForCallMethod(obj.GetTypeAdjusted(), genericTypes, name, bindingFlags)(obj, parameters);
        }

        /// <summary>
        /// Invokes a method specified by <paramref name="name"/> on the given <paramref name="obj"/> 
        /// matching <paramref name="bindingFlags"/> using <paramref name="parameters"/> as arguments.
        /// Method parameter types are specified by <paramref name="parameterTypes"/>.
        /// </summary>
        /// <returns>The return value of the method.</returns>
        /// <remarks>If the method has no return type, <c>null</c> is returned.</remarks>
        public static object CallMethod(this object obj, string name, Type[] parameterTypes, Flags bindingFlags, params object[] parameters)
        {
            return DelegateForCallMethod(obj.GetTypeAdjusted(), name, bindingFlags, parameterTypes)(obj, parameters);
        }

        /// <summary>
        /// Invoke generic method.  See the overload with same parameters except for <paramref name="genericTypes"/>.
        /// </summary>
        /// <seealso cref="CallMethod(object,string,Type[],Flags,object[])"/>
        public static object CallMethod(this object obj, Type[] genericTypes, string name, Type[] parameterTypes, Flags bindingFlags, params object[] parameters)
        {
            return DelegateForCallMethod(obj.GetTypeAdjusted(), genericTypes, name, bindingFlags, parameterTypes)(obj, parameters);
        }

        /// <summary>
        /// Creates a delegate which can invoke the method <paramref name="name"/> with arguments matching
        /// <paramref name="parameterTypes"/> on the given <paramref name="type"/>.
        /// Leave <paramref name="parameterTypes"/> empty if the method has no arguments.
        /// </summary>
        public static MethodInvoker DelegateForCallMethod( this Type type, string name, params Type[] parameterTypes )
        {
            return DelegateForCallMethod( type, name, Flags.StaticInstanceAnyVisibility, parameterTypes );
        }

        /// <summary>
        /// Create a delegate to invoke a generic method.  See the overload with same parameters except for <paramref name="genericTypes"/>.
        /// </summary>
        /// <seealso cref="DelegateForCallMethod(Type,string,Type[])"/>
        public static MethodInvoker DelegateForCallMethod(this Type type, Type[] genericTypes, string name, params Type[] parameterTypes)
        {
            return DelegateForCallMethod(type, genericTypes, name, Flags.StaticInstanceAnyVisibility, parameterTypes);
        }

        /// <summary>
        /// Creates a delegate which can invoke the method <paramref name="name"/> with arguments matching
        /// <paramref name="parameterTypes"/> and matching <paramref name="bindingFlags"/> on the given <paramref name="type"/>.
        /// Leave <paramref name="parameterTypes"/> empty if the method has no arguments.
        /// </summary>
        public static MethodInvoker DelegateForCallMethod( this Type type, string name, Flags bindingFlags, params Type[] parameterTypes )
        {
            return DelegateForCallMethod(type, null, name, bindingFlags, parameterTypes);
        }

        /// <summary>
        /// Create a delegate to invoke a generic method.  See the overload with same parameters except for <paramref name="genericTypes"/>.
        /// </summary>
        /// <seealso cref="DelegateForCallMethod(Type,string,Flags,Type[])"/>
        public static MethodInvoker DelegateForCallMethod(this Type type, Type[] genericTypes, string name, Flags bindingFlags, params Type[] parameterTypes)
        {
            var callInfo = new CallInfo(type, genericTypes, bindingFlags, MemberTypes.Method, name, parameterTypes, null, true);
            return (MethodInvoker)new MethodInvocationEmitter(callInfo).GetDelegate();
        }
		#endregion

        #region Method Lookup (Single)
        /// <summary>
        /// Gets the public or non-public instance method with the given <paramref name="name"/> on the
        /// given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type on which to reflect.</param>
        /// <param name="name">The name of the method to search for. This argument must be supplied. The 
        /// default behavior is to check for an exact, case-sensitive match. Pass <see href="Flags.ExplicitNameMatch"/> 
        /// to include explicitly implemented interface members, <see href="Flags.PartialNameMatch"/> to locate
        /// by substring, and <see href="Flags.IgnoreCase"/> to ignore case.</param>
        /// <returns>The specified method or null if no method was found. If there are multiple matches
        /// due to method overloading the first found match will be returned.</returns>
        public static MethodInfo Method( this Type type, string name )
        {
            return type.Method( name, null, Flags.InstanceAnyVisibility );
        }

        /// <summary>
        /// Gets a generic method.  See the overload with same arguments exception for <param name="genericTypes"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="genericTypes">The generic types.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <seealso cref="Method(Type,string)"/>
        public static MethodInfo Method(this Type type, Type[] genericTypes, string name)
        {
            return type.Method(genericTypes, name, Flags.InstanceAnyVisibility);
        }

        /// <summary>
        /// Gets the public or non-public instance method with the given <paramref name="name"/> on the 
        /// given <paramref name="type"/> where the parameter types correspond in order with the
        /// supplied <paramref name="parameterTypes"/>.
        /// </summary>
        /// <param name="type">The type on which to reflect.</param>
        /// <param name="name">The name of the method to search for. This argument must be supplied. The 
        /// default behavior is to check for an exact, case-sensitive match.</param>
        /// <param name="parameterTypes">If this parameter is not null then only methods with the same 
        /// parameter signature will be included in the result.</param>
        /// <returns>The specified method or null if no method was found. If there are multiple matches
        /// due to method overloading the first found match will be returned.</returns>
        public static MethodInfo Method( this Type type, string name, Type[] parameterTypes )
        {
        	return type.Method( name, parameterTypes, Flags.InstanceAnyVisibility );
        }

        /// <summary>
        /// Gets a generic method.  See the overload with same arguments exception for <param name="genericTypes"/>.
        /// </summary>
        /// <seealso cref="Method(Type,string,Type[])"/>
        public static MethodInfo Method(this Type type, Type[] genericTypes, string name, Type[] parameterTypes)
        {
            return type.Method(genericTypes, name, parameterTypes, Flags.InstanceAnyVisibility);
        }

    	/// <summary>
        /// Gets the method with the given <paramref name="name"/> and matching <paramref name="bindingFlags"/>
        /// on the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type on which to reflect.</param>
        /// <param name="name">The name of the method to search for. This argument must be supplied. The 
        /// default behavior is to check for an exact, case-sensitive match. Pass <see href="Flags.ExplicitNameMatch"/> 
        /// to include explicitly implemented interface members, <see href="Flags.PartialNameMatch"/> to locate
        /// by substring, and <see href="Flags.IgnoreCase"/> to ignore case.</param>
        /// <param name="bindingFlags">The <see cref="BindingFlags"/> or <see cref="Flags"/> combination used to define
        /// the search behavior and result filtering.</param>
        /// <returns>The specified method or null if no method was found. If there are multiple matches
        /// due to method overloading the first found match will be returned.</returns>
        public static MethodInfo Method( this Type type, string name, Flags bindingFlags )
        {
            return type.Method( name, null, bindingFlags );
        }

        /// <summary>
        /// Gets a generic method.  See the overload with same arguments exception for <param name="genericTypes"/>.
        /// </summary>
        /// <seealso cref="Method(Type,string,Flags)"/>
        public static MethodInfo Method(this Type type, Type[] genericTypes, string name, Flags bindingFlags)
        {
            return type.Method(genericTypes, name, null, bindingFlags);
        }

        /// <summary>
        /// Gets the method with the given <paramref name="name"/> and matching <paramref name="bindingFlags"/>
        /// on the given <paramref name="type"/> where the parameter types correspond in order with the
        /// supplied <paramref name="parameterTypes"/>.
        /// </summary>
        /// <param name="type">The type on which to reflect.</param>
        /// <param name="name">The name of the method to search for. This argument must be supplied. The 
        ///   default behavior is to check for an exact, case-sensitive match. Pass <see href="Flags.ExplicitNameMatch"/> 
        ///   to include explicitly implemented interface members, <see href="Flags.PartialNameMatch"/> to locate
        ///   by substring, and <see href="Flags.IgnoreCase"/> to ignore case.</param>
        /// <param name="parameterTypes">If this parameter is supplied then only methods with the same parameter signature
        ///   will be included in the result. The default behavior is to check only for assignment compatibility,
        ///   but this can be changed to exact matching by passing <see href="Flags.ExactBinding"/>.</param>
        /// <param name="bindingFlags">The <see cref="BindingFlags"/> or <see cref="Flags"/> combination used to define
        ///   the search behavior and result filtering.</param>
        /// <returns>The specified method or null if no method was found. If there are multiple matches
        /// due to method overloading the first found match will be returned.</returns>
        public static MethodInfo Method( this Type type, string name, Type[] parameterTypes, Flags bindingFlags )
        {
            return type.Method(null, name, parameterTypes, bindingFlags);
        }

        /// <summary>
        /// Gets the method with the given <paramref name="name"/> and matching <paramref name="bindingFlags"/>
        /// on the given <paramref name="type"/> where the parameter types correspond in order with the
        /// supplied <paramref name="parameterTypes"/>.
        /// </summary>
        /// <param name="type">The type on which to reflect.</param>
        /// <param name="genericTypes">Type parameters if this is a generic method.</param>
        /// <param name="name">The name of the method to search for. This argument must be supplied. The 
        ///   default behavior is to check for an exact, case-sensitive match. Pass <see href="Flags.ExplicitNameMatch"/> 
        ///   to include explicitly implemented interface members, <see href="Flags.PartialNameMatch"/> to locate
        ///   by substring, and <see href="Flags.IgnoreCase"/> to ignore case.</param>
        /// <param name="parameterTypes">If this parameter is supplied then only methods with the same parameter signature
        ///   will be included in the result. The default behavior is to check only for assignment compatibility,
        ///   but this can be changed to exact matching by passing <see href="Flags.ExactBinding"/>.</param>
        /// <param name="bindingFlags">The <see cref="BindingFlags"/> or <see cref="Flags"/> combination used to define
        ///   the search behavior and result filtering.</param>
        /// <returns>The specified method or null if no method was found. If there are multiple matches
        /// due to method overloading the first found match will be returned.</returns>
        public static MethodInfo Method( this Type type, Type[] genericTypes, string name, Type[] parameterTypes, Flags bindingFlags )
        {
			bool hasTypes = parameterTypes != null;
        	bool hasGenericTypes = genericTypes != null && genericTypes.Length > 0;
            // we need to check all methods to do partial name matches or complex parameter binding
        	bool processAll = bindingFlags.IsAnySet( Flags.PartialNameMatch | Flags.TrimExplicitlyImplemented );
        	processAll |= hasTypes && bindingFlags.IsSet( Flags.IgnoreParameterModifiers );
        	processAll |= hasGenericTypes;
            if( processAll )
            {
                return type.Methods( genericTypes, parameterTypes, bindingFlags, name ).FirstOrDefault().MakeGeneric( genericTypes );
            }

            var result = hasTypes ? type.GetMethod( name, bindingFlags, null, parameterTypes, null )
                             	  : type.GetMethod( name, bindingFlags );
            if( result == null && bindingFlags.IsNotSet( Flags.DeclaredOnly ) )
            {
                if( type.BaseType != typeof(object) && type.BaseType != null )
                {
                    return type.BaseType.Method( name, parameterTypes, bindingFlags ).MakeGeneric( genericTypes );
                }
            }
        	bool hasSpecialFlags = bindingFlags.IsAnySet( Flags.ExcludeBackingMembers | Flags.ExcludeExplicitlyImplemented );
            if( hasSpecialFlags )
            {
                var methods = new List<MethodInfo> { result }.Filter( bindingFlags );
                return (methods.Count > 0 ? methods[ 0 ] : null).MakeGeneric( genericTypes );
            }
            return result.MakeGeneric(genericTypes);
        }

        internal static MethodInfo MakeGeneric(this MethodInfo methodInfo, Type[] genericTypes)
        {
            if (methodInfo == null)
                return null;
            if (genericTypes == null ||
                genericTypes.Length == 0 ||
                genericTypes == Type.EmptyTypes)
                return methodInfo;
            return methodInfo.MakeGenericMethod( genericTypes );
        }
        #endregion

        #region Method Lookup (Multiple)
        /// <summary>
        /// Gets all public and non-public instance methods on the given <paramref name="type"/> that match the 
        /// given <paramref name="names"/>.
        /// </summary>
        /// <param name="type">The type on which to reflect.</param>
        /// <param name="names">The optional list of names against which to filter the result. If this parameter is
		/// <c>null</c> or empty no name filtering will be applied. The default behavior is to check for an exact, 
		/// case-sensitive match. Pass <see href="Flags.ExcludeExplicitlyImplemented"/> to exclude explicitly implemented 
		/// interface members, <see href="Flags.PartialNameMatch"/> to locate by substring, and 
		/// <see href="Flags.IgnoreCase"/> to ignore case.</param>
        /// <returns>A list of all matching methods. This value will never be null.</returns>
        public static IList<MethodInfo> Methods( this Type type, params string[] names )
        {
            return type.Methods( null, Flags.InstanceAnyVisibility, names );
        }

        /// <summary>
        /// Gets all public and non-public instance methods on the given <paramref name="type"/> that match the 
        /// given <paramref name="names"/>.
        /// </summary>
        /// <param name="type">The type on which to reflect.</param>
        /// <param name="bindingFlags">The <see cref="BindingFlags"/> or <see cref="Flags"/> combination used to define
        /// the search behavior and result filtering.</param>
        /// <param name="names">The optional list of names against which to filter the result. If this parameter is
		/// <c>null</c> or empty no name filtering will be applied. The default behavior is to check for an exact, 
		/// case-sensitive match. Pass <see href="Flags.ExcludeExplicitlyImplemented"/> to exclude explicitly implemented 
		/// interface members, <see href="Flags.PartialNameMatch"/> to locate by substring, and 
		/// <see href="Flags.IgnoreCase"/> to ignore case.</param>
        /// <returns>A list of all matching methods. This value will never be null.</returns>
        public static IList<MethodInfo> Methods( this Type type, Flags bindingFlags, params string[] names )
        {
            return type.Methods( null, bindingFlags, names );
        }


        /// <summary>
        /// Gets all public and non-public instance methods on the given <paramref name="type"/> that match the given 
        ///  <paramref name="names"/>.
        /// </summary>
        /// <param name="type">The type on which to reflect.</param>
        /// <param name="parameterTypes">If this parameter is supplied then only methods with the same parameter 
        /// signature will be included in the result.</param>
        /// <param name="names">The optional list of names against which to filter the result. If this parameter is
		/// <c>null</c> or empty no name filtering will be applied. The default behavior is to check for an exact, 
		/// case-sensitive match.</param>
        /// <returns>A list of all matching methods. This value will never be null.</returns>
        public static IList<MethodInfo> Methods( this Type type, Type[] parameterTypes, params string[] names )
        {
        	return type.Methods( parameterTypes, Flags.InstanceAnyVisibility, names );
        }

    	/// <summary>
        /// Gets all methods on the given <paramref name="type"/> that match the given lookup criteria and values.
        /// </summary>
        /// <param name="type">The type on which to reflect.</param>
        /// <param name="parameterTypes">If this parameter is supplied then only methods with the same parameter signature
        /// will be included in the result. The default behavior is to check only for assignment compatibility,
        /// but this can be changed to exact matching by passing <see href="Flags.ExactBinding"/>.</param>
        /// <param name="bindingFlags">The <see cref="BindingFlags"/> or <see cref="Flags"/> combination used to define
        /// the search behavior and result filtering.</param>
        /// <param name="names">The optional list of names against which to filter the result. If this parameter is
		/// <c>null</c> or empty no name filtering will be applied. The default behavior is to check for an exact, 
		/// case-sensitive match. Pass <see href="Flags.ExcludeExplicitlyImplemented"/> to exclude explicitly implemented 
		/// interface members, <see href="Flags.PartialNameMatch"/> to locate by substring, and 
		/// <see href="Flags.IgnoreCase"/> to ignore case.</param>
        /// <returns>A list of all matching methods. This value will never be null.</returns>
        public static IList<MethodInfo> Methods( this Type type, Type[] parameterTypes, Flags bindingFlags, params string[] names )
    	{
    		return type.Methods( null, parameterTypes, bindingFlags, names );
    	}

    	public static IList<MethodInfo> Methods( this Type type, Type[] genericTypes, Type[] parameterTypes, Flags bindingFlags, params string[] names )
        {
            if( type == null || type == typeof(object) )
            {
                return new MethodInfo[0];
            }
			bool recurse = bindingFlags.IsNotSet( Flags.DeclaredOnly );
            bool hasNames = names != null && names.Length > 0;
            bool hasTypes = parameterTypes != null;
            bool hasGenericTypes = genericTypes != null && genericTypes.Length > 0;
            bool hasSpecialFlags = bindingFlags.IsAnySet( Flags.ExcludeBackingMembers | Flags.ExcludeExplicitlyImplemented );

            if( ! recurse && ! hasNames && ! hasTypes && ! hasSpecialFlags )
            {
                return type.GetMethods( bindingFlags ) ?? new MethodInfo[0];
            }

            var methods = GetMethods( type, bindingFlags );
            methods = hasNames ? methods.Filter(bindingFlags, names) : methods;
            methods = hasGenericTypes ? methods.Filter(genericTypes) : methods;
            methods = hasTypes ? methods.Filter( bindingFlags, parameterTypes ) : methods;
            methods = hasSpecialFlags ? methods.Filter( bindingFlags ) : methods;
            return methods;
        }

        private static IList<MethodInfo> GetMethods( Type type, Flags bindingFlags )
        {
            bool recurse = bindingFlags.IsNotSet( Flags.DeclaredOnly );

            if( ! recurse )
            {
                return type.GetMethods( bindingFlags ) ?? new MethodInfo[0];
            }

            bindingFlags |= Flags.DeclaredOnly;
            bindingFlags &= ~BindingFlags.FlattenHierarchy;

            var methods = new List<MethodInfo>();
            methods.AddRange( type.GetMethods( bindingFlags ) );
            Type baseType = type.BaseType;
            while( baseType != null && baseType != typeof(object) )
            {
                methods.AddRange( baseType.GetMethods( bindingFlags ) );
                baseType = baseType.BaseType;
            }
            return methods;
        }
        #endregion
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Core\MethodInfoExtensions.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect
{
    using System.Collections.Generic;
    using System.Reflection;
    using Fasterflect.Emitter;

    /// <summary>
    /// Extension methods for inspecting, invoking and working with methods.
    /// </summary>
    internal static class MethodInfoExtensions
    {
        #region Access
        /// <summary>
        /// Invokes the static method identified by <paramref name="methodInfo"/> with <paramref name="parameters"/>
        /// as arguments.  Leave <paramref name="parameters"/> empty if the method has no argument.
        /// </summary>
        /// <returns>The return value of the method.</returns>
        /// <remarks>If the method has no return type, <c>null</c> is returned.</remarks>
        public static object Call( this MethodInfo methodInfo, params object[] parameters )
        {
            return methodInfo.DelegateForCallMethod()( null, parameters );
        }

        /// <summary>
        /// Invokes the instance method identified by <paramref name="methodInfo"/> on the object
        /// <paramref name="obj"/> with <paramref name="parameters"/> as arguments.
        /// Leave <paramref name="parameters"/> empty if the method has no argument.
        /// </summary>
        /// <returns>The return value of the method.</returns>
        /// <remarks>If the method has no return type, <c>null</c> is returned.</remarks>
        public static object Call( this MethodInfo methodInfo, object obj, params object[] parameters )
        {
            return methodInfo.DelegateForCallMethod()( obj, parameters );
        }

        /// <summary>
        /// Creates a delegate which can invoke the instance method identified by <paramref name="methodInfo"/>.
        /// </summary>
        public static MethodInvoker DelegateForCallMethod( this MethodInfo methodInfo )
        {
		    var flags = methodInfo.IsStatic ? Flags.StaticAnyVisibility : Flags.InstanceAnyVisibility;
            return (MethodInvoker) new MethodInvocationEmitter( methodInfo, flags ).GetDelegate();
        }
        #endregion

        #region Method Parameter Lookup
        /// <summary>
        /// Gets all parameters for the given <paramref name="method"/>.
        /// </summary>
        /// <returns>The list of parameters for the method. This value will never be null.</returns>
        public static IList<ParameterInfo> Parameters( this MethodBase method )
        {
            return method.GetParameters();
        }
        #endregion
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Core\ParameterInfoExtensions.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect
{
    using System;
    using System.ComponentModel;
    using System.Reflection;

    /// <summary>
    /// Extension methods for inspecting and working with method parameters.
    /// </summary>
    internal static class ParameterInfoExtensions
    {
        /// <summary>
        /// Determines whether null can be assigned to the given <paramref name="parameter"/>.
        /// </summary>
        /// <returns>True if null can be assigned, false otherwise.</returns>
        public static bool IsNullable( this ParameterInfo parameter )
        {
            return ! parameter.ParameterType.IsValueType || parameter.ParameterType.IsSubclassOf( typeof(Nullable) );
        }

        /// <summary>
        /// Determines whether the given <paramref name="parameter"/> has the given <paramref name="name"/>.
        /// The comparison uses OrdinalIgnoreCase and allows for a leading underscore in either name
        /// to be ignored.
        /// </summary>
        /// <returns>True if the name is considered identical, false otherwise. If either parameter
        /// is null an exception will be thrown.</returns>
        public static bool HasName( this ParameterInfo parameter, string name )
        {
            string parameterName = parameter.Name.Length > 0 && parameter.Name[ 0 ] == '_'
                                       ? parameter.Name.Substring( 1 )
                                       : parameter.Name;
            name = name.Length > 0 && name[ 0 ] == '_' ? name.Substring( 1 ) : name;
            return parameterName.Equals( name, StringComparison.OrdinalIgnoreCase );
        }

        /// <summary>
        /// Determines whether the given <paramref name="parameter"/> has an associated default value as
        /// supplied by an <see href="DefaultValueAttribute"/>. This method does not read the value of
        /// the attribute. It also does not support C# 4.0 default parameter specifications.
        /// </summary>
        /// <returns>True if the attribute was detected, false otherwise.</returns>
        public static bool HasDefaultValue( this ParameterInfo parameter )
        {
            var defaultValue = parameter.Attribute<DefaultValueAttribute>();
            return defaultValue != null;
        }

        /// <summary>
        /// Gets the default value associated with the given <paramref name="parameter"/>. The value is
        /// obtained from the <see href="DefaultValueAttribute"/> if present on the parameter. This method 
        /// does not support C# 4.0 default parameter specifications.
        /// </summary>
        /// <returns>The default value if one could be obtained and converted into the type of the parameter,
        /// and null otherwise.</returns>
        public static object DefaultValue( this ParameterInfo parameter )
        {
            var defaultValue = parameter.Attribute<DefaultValueAttribute>();
            return defaultValue != null
                       ? Probing.TypeConverter.Get( parameter.ParameterType, defaultValue.Value )
                       : null;
        }
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Core\PropertyExtensions.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Fasterflect.Emitter;

    /// <summary>
    /// Extension methods for locating and accessing properties.
    /// </summary>
    internal static class PropertyExtensions
    {
        #region Property Access
        /// <summary>
        /// Sets the property specified by <param name="name"/> on the given <param name="obj"/> to the 
        /// specified <param name="value" />.
        /// </summary>
        /// <returns><paramref name="obj"/>.</returns>
        public static object SetPropertyValue( this object obj, string name, object value )
        {
            DelegateForSetPropertyValue( obj.GetTypeAdjusted(), name )( obj, value );
            return obj;
        }

        /// <summary>
        /// Gets the value of the property specified by <param name="name"/> on the given <param name="obj"/>.
        /// </summary>
        public static object GetPropertyValue( this object obj, string name )
        {
            return DelegateForGetPropertyValue( obj.GetTypeAdjusted(), name )( obj );
        }

        /// <summary>
        /// Sets the property specified by <param name="name"/> matching <param name="bindingFlags"/>
        /// on the given <param name="obj"/> to the specified <param name="value" />.
        /// </summary>
        /// <returns><paramref name="obj"/>.</returns>
        public static object SetPropertyValue( this object obj, string name, object value, Flags bindingFlags )
        {
            DelegateForSetPropertyValue( obj.GetTypeAdjusted(), name, bindingFlags )( obj, value );
            return obj;
        }

        /// <summary>
        /// Gets the value of the property specified by <param name="name"/> matching <param name="bindingFlags"/>
        /// on the given <param name="obj"/>.
        /// </summary>
        public static object GetPropertyValue( this object obj, string name, Flags bindingFlags )
        {
            return DelegateForGetPropertyValue( obj.GetTypeAdjusted(), name, bindingFlags )( obj );
        }

        /// <summary>
        /// Sets the property specified by <param name="memberExpression"/> on the given <param name="obj"/> to the 
        /// specified <param name="value" />.
        /// </summary>
        /// <returns><paramref name="obj"/>.</returns>
        public static object SetPropertyValue( this object obj, Expression<Func<object>> memberExpression, object value )
        {
        	var body = memberExpression != null ? memberExpression.Body as MemberExpression : null;
			if( body == null || body.Member == null )
			{
				throw new ArgumentNullException( "memberExpression" );
			}
        	return obj.SetPropertyValue( body.Member.Name, value );
        }

        /// <summary>
        /// Gets the value of the property specified by <param name="memberExpression"/> on the given <param name="obj"/>.
        /// </summary>
        public static object GetPropertyValue( this object obj, Expression<Func<object>> memberExpression )
        {
        	var body = memberExpression != null ? memberExpression.Body as MemberExpression : null;
			if( body == null || body.Member == null )
			{
				throw new ArgumentNullException( "memberExpression" );
			}
        	return obj.GetPropertyValue( body.Member.Name );
        }

        /// <summary>
        /// Creates a delegate which can set the value of the property specified by <param name="name"/>
        /// on the given <param name="type"/>.
        /// </summary>
        public static MemberSetter DelegateForSetPropertyValue( this Type type, string name )
        {
            return DelegateForSetPropertyValue( type, name, Flags.StaticInstanceAnyVisibility );
        }

        /// <summary>
        /// Creates a delegate which can get the value of the property specified by <param name="name"/>
        /// on the given <param name="type"/>.
        /// </summary>
        public static MemberGetter DelegateForGetPropertyValue( this Type type, string name )
        {
            return DelegateForGetPropertyValue( type, name, Flags.StaticInstanceAnyVisibility );
        }

        /// <summary>
        /// Creates a delegate which can set the value of the property specified by <param name="name"/>
        /// matching <param name="bindingFlags"/> on the given <param name="type"/>.
        /// </summary>
        public static MemberSetter DelegateForSetPropertyValue( this Type type, string name, Flags bindingFlags )
        {
            var callInfo = new CallInfo(type, null, bindingFlags, MemberTypes.Property, name, null, null, false);
			return (MemberSetter) new MemberSetEmitter( callInfo ).GetDelegate();
        }

        /// <summary>
        /// Creates a delegate which can get the value of the property specified by <param name="name"/>
        /// matching <param name="bindingFlags"/> on the given <param name="type"/>.
        /// </summary>
        public static MemberGetter DelegateForGetPropertyValue( this Type type, string name, Flags bindingFlags )
        {
            var callInfo = new CallInfo(type, null, bindingFlags, MemberTypes.Property, name, null, null, true);
			return (MemberGetter) new MemberGetEmitter( callInfo ).GetDelegate();
        }
    	#endregion

        #region Indexer Access
        /// <summary>
        /// Sets the value of the indexer of the given <paramref name="obj"/>
        /// </summary>
        /// <param name="obj">The object whose indexer is to be set.</param>
        /// <param name="parameters">The list of the indexer parameters plus the value to be set to the indexer.
        /// The parameter types are determined from these parameters, therefore no parameter can be <c>null</c>.
        /// If any parameter is <c>null</c> (or you can't be sure of that, i.e. receive from a variable), 
        /// use a different overload of this method.</param>
        /// <returns>The object whose indexer is to be set.</returns>
        /// <example>
        /// If the indexer is of type <c>string</c> and accepts one parameter of type <c>int</c>, this 
        /// method should be invoked as follow:
        /// <code>
        /// obj.SetIndexer(new Type[]{typeof(int), typeof(string)}, new object[]{1, "a"});
        /// </code>
        /// </example>
        public static object SetIndexer( this object obj, params object[] parameters )
        {
            DelegateForSetIndexer( obj.GetTypeAdjusted(), parameters.ToTypeArray() )( obj, parameters );
            return obj;
        }

        /// <summary>
        /// Sets the value of the indexer of the given <paramref name="obj"/>
        /// </summary>
        /// <param name="obj">The object whose indexer is to be set.</param>
        /// <param name="parameterTypes">The types of the indexer parameters (must be in the right order), plus
        /// the type of the indexer.</param>
        /// <param name="parameters">The list of the indexer parameters plus the value to be set to the indexer.
        /// This list must match with the <paramref name="parameterTypes"/> list.</param>
        /// <returns>The object whose indexer is to be set.</returns>
        /// <example>
        /// If the indexer is of type <c>string</c> and accepts one parameter of type <c>int</c>, this 
        /// method should be invoked as follow:
        /// <code>
        /// obj.SetIndexer(new Type[]{typeof(int), typeof(string)}, new object[]{1, "a"});
        /// </code>
        /// </example>
        public static object SetIndexer( this object obj, Type[] parameterTypes, params object[] parameters )
        {
            DelegateForSetIndexer( obj.GetTypeAdjusted(), parameterTypes )( obj, parameters );
            return obj;
        }

        /// <summary>
        /// Gets the value of the indexer of the given <paramref name="obj"/>
        /// </summary>
        /// <param name="obj">The object whose indexer is to be retrieved.</param>
        /// <param name="parameters">The list of the indexer parameters.
        /// The parameter types are determined from these parameters, therefore no parameter can be <code>null</code>.
        /// If any parameter is <code>null</code> (or you can't be sure of that, i.e. receive from a variable), 
        /// use a different overload of this method.</param>
        /// <returns>The value returned by the indexer.</returns>
        public static object GetIndexer( this object obj, params object[] parameters )
        {
            return DelegateForGetIndexer( obj.GetTypeAdjusted(), parameters.ToTypeArray() )( obj, parameters );
        }

        /// <summary>
        /// Gets the value of the indexer of the given <paramref name="obj"/>
        /// </summary>
        /// <param name="obj">The object whose indexer is to be retrieved.</param>
        /// <param name="parameterTypes">The types of the indexer parameters (must be in the right order).</param>
        /// <param name="parameters">The list of the indexer parameters.</param>
        /// <returns>The value returned by the indexer.</returns>
        public static object GetIndexer( this object obj, Type[] parameterTypes, params object[] parameters )
        {
            return DelegateForGetIndexer( obj.GetTypeAdjusted(), parameterTypes )( obj, parameters );
        }

        /// <summary>
        /// Sets the value of the indexer matching <paramref name="bindingFlags"/> of the given <paramref name="obj"/>
        /// </summary>
        /// <param name="obj">The object whose indexer is to be set.</param>
        /// <param name="bindingFlags">The binding flags used to lookup the indexer.</param>
        /// <param name="parameters">The list of the indexer parameters plus the value to be set to the indexer.
        /// The parameter types are determined from these parameters, therefore no parameter can be <c>null</c>.
        /// If any parameter is <c>null</c> (or you can't be sure of that, i.e. receive from a variable), 
        /// use a different overload of this method.</param>
        /// <returns>The object whose indexer is to be set.</returns>
        /// <example>
        /// If the indexer is of type <c>string</c> and accepts one parameter of type <c>int</c>, this 
        /// method should be invoked as follow:
        /// <code>
        /// obj.SetIndexer(new Type[]{typeof(int), typeof(string)}, new object[]{1, "a"});
        /// </code>
        /// </example>
        public static object SetIndexer( this object obj, Flags bindingFlags, params object[] parameters )
        {
            DelegateForSetIndexer( obj.GetTypeAdjusted(), bindingFlags, parameters.ToTypeArray() )( obj,
                                                                                                        parameters );
            return obj;
        }

        /// <summary>
        /// Sets the value of the indexer matching <paramref name="bindingFlags"/> of the given <paramref name="obj"/>
        /// </summary>
        /// <param name="obj">The object whose indexer is to be set.</param>
        /// <param name="parameterTypes">The types of the indexer parameters (must be in the right order), plus
        ///   the type of the indexer.</param>
        /// <param name="bindingFlags">The binding flags used to lookup the indexer.</param>
        /// <param name="parameters">The list of the indexer parameters plus the value to be set to the indexer.
        ///   This list must match with the <paramref name="parameterTypes"/> list.</param>
        /// <returns>The object whose indexer is to be set.</returns>
        /// <example>
        /// If the indexer is of type <c>string</c> and accepts one parameter of type <c>int</c>, this 
        /// method should be invoked as follow:
        /// <code>
        /// obj.SetIndexer(new Type[]{typeof(int), typeof(string)}, new object[]{1, "a"});
        /// </code>
        /// </example>
        public static object SetIndexer( this object obj, Type[] parameterTypes, Flags bindingFlags, params object[] parameters )
        {
            DelegateForSetIndexer( obj.GetTypeAdjusted(), bindingFlags, parameterTypes )( obj, parameters );
            return obj;
        }

        /// <summary>
        /// Gets the value of the indexer matching <paramref name="bindingFlags"/> of the given <paramref name="obj"/>
        /// </summary>
        /// <param name="obj">The object whose indexer is to be retrieved.</param>
        /// <param name="bindingFlags">The binding flags used to lookup the indexer.</param>
        /// <param name="parameters">The list of the indexer parameters.
        /// The parameter types are determined from these parameters, therefore no parameter can be <code>null</code>.
        /// If any parameter is <code>null</code> (or you can't be sure of that, i.e. receive from a variable), 
        /// use a different overload of this method.</param>
        /// <returns>The value returned by the indexer.</returns>
        public static object GetIndexer( this object obj, Flags bindingFlags, params object[] parameters )
        {
            return DelegateForGetIndexer( obj.GetTypeAdjusted(), bindingFlags, parameters.ToTypeArray() )( obj,
                                                                                                               parameters );
        }

        /// <summary>
        /// Gets the value of the indexer matching <paramref name="bindingFlags"/> of the given <paramref name="obj"/>
        /// </summary>
        /// <param name="obj">The object whose indexer is to be retrieved.</param>
        /// <param name="parameterTypes">The types of the indexer parameters (must be in the right order).</param>
        /// <param name="bindingFlags">The binding flags used to lookup the indexer.</param>
        /// <param name="parameters">The list of the indexer parameters.</param>
        /// <returns>The value returned by the indexer.</returns>
        public static object GetIndexer( this object obj, Type[] parameterTypes, Flags bindingFlags, params object[] parameters )
        {
            return DelegateForGetIndexer( obj.GetTypeAdjusted(), bindingFlags, parameterTypes )( obj, parameters );
        }

        /// <summary>
        /// Creates a delegate which can set an indexer
        /// </summary>
        /// <param name="type">The type which the indexer belongs to.</param>
        /// <param name="parameterTypes">The types of the indexer parameters (must be in the right order), plus
        /// the type of the indexer.</param>
        /// <returns>A delegate which can set an indexer.</returns>
        /// <example>
        /// If the indexer is of type <c>string</c> and accepts one parameter of type <c>int</c>, this 
        /// method should be invoked as follow:
        /// <code>
        /// MethodInvoker invoker = type.DelegateForSetIndexer(new Type[]{typeof(int), typeof(string)});
        /// </code>
        /// </example>
        public static MethodInvoker DelegateForSetIndexer( this Type type, params Type[] parameterTypes )
        {
            return DelegateForSetIndexer( type, Flags.InstanceAnyVisibility, parameterTypes );
        }

        /// <summary>
        /// Creates a delegate which can get the value of an indexer.
        /// </summary>
        /// <param name="type">The type which the indexer belongs to.</param>
        /// <param name="parameterTypes">The types of the indexer parameters (must be in the right order).</param>
        /// <returns>The delegate which can get the value of an indexer.</returns>
        public static MethodInvoker DelegateForGetIndexer( this Type type, params Type[] parameterTypes )
        {
            return DelegateForGetIndexer( type, Flags.InstanceAnyVisibility, parameterTypes );
        }

        /// <summary>
        /// Creates a delegate which can set an indexer matching <paramref name="bindingFlags"/>.
        /// </summary>
        /// <param name="type">The type which the indexer belongs to.</param>
        /// <param name="bindingFlags">The binding flags used to lookup the indexer.</param>
        /// <param name="parameterTypes">The types of the indexer parameters (must be in the right order), plus
        /// the type of the indexer.</param>
        /// <returns>A delegate which can set an indexer.</returns>
        /// <example>
        /// If the indexer is of type <c>string</c> and accepts one parameter of type <c>int</c>, this 
        /// method should be invoked as follow:
        /// <code>
        /// MethodInvoker invoker = type.DelegateForSetIndexer(new Type[]{typeof(int), typeof(string)});
        /// </code>
        /// </example>
        public static MethodInvoker DelegateForSetIndexer( this Type type, Flags bindingFlags,
                                                           params Type[] parameterTypes )
        {
            return (MethodInvoker)
                new MethodInvocationEmitter( type, bindingFlags, Constants.IndexerSetterName, parameterTypes ).
                    GetDelegate();
        }

        /// <summary>
        /// Creates a delegate which can get the value of an indexer matching <paramref name="bindingFlags"/>.
        /// </summary>
        /// <param name="type">The type which the indexer belongs to.</param>
        /// <param name="bindingFlags">The binding flags used to lookup the indexer.</param>
        /// <param name="parameterTypes">The types of the indexer parameters (must be in the right order).</param>
        /// <returns>The delegate which can get the value of an indexer.</returns>
        public static MethodInvoker DelegateForGetIndexer( this Type type, Flags bindingFlags,
                                                           params Type[] parameterTypes )
        {
            return (MethodInvoker)
                new MethodInvocationEmitter( type, bindingFlags, Constants.IndexerGetterName, parameterTypes ).
                    GetDelegate();
        }
        #endregion

        #region Property Lookup (Single)
        /// <summary>
        /// Gets the property identified by <paramref name="name"/> on the given <paramref name="type"/>. This method 
        /// searches for public and non-public instance properties on both the type itself and all parent classes.
        /// </summary>
        /// <returns>A single PropertyInfo instance of the first found match or null if no match was found.</returns>
        public static PropertyInfo Property( this Type type, string name )
        {
            return type.Property( name, Flags.InstanceAnyVisibility );
        }

        /// <summary>
        /// Gets the property identified by <paramref name="name"/> on the given <paramref name="type"/>. 
        /// Use the <paramref name="bindingFlags"/> parameter to define the scope of the search.
        /// </summary>
        /// <returns>A single PropertyInfo instance of the first found match or null if no match was found.</returns>
        public static PropertyInfo Property( this Type type, string name, Flags bindingFlags )
        {
            // we need to check all properties to do partial name matches
            if( bindingFlags.IsAnySet( Flags.PartialNameMatch | Flags.TrimExplicitlyImplemented ) )
            {
                return type.Properties( bindingFlags, name ).FirstOrDefault();
            }

            var result = type.GetProperty( name, bindingFlags );
            if( result == null && bindingFlags.IsNotSet( Flags.DeclaredOnly ) )
            {
                if( type.BaseType != typeof(object) && type.BaseType != null )
                {
                    return type.BaseType.Property( name, bindingFlags );
                }
            }
            bool hasSpecialFlags = bindingFlags.IsSet( Flags.ExcludeExplicitlyImplemented );
            if( hasSpecialFlags )
            {
                IList<PropertyInfo> properties = new List<PropertyInfo> { result };
                properties = properties.Filter( bindingFlags );
                return properties.Count > 0 ? properties[ 0 ] : null;
            }
            return result;
        }
        #endregion

        #region Property Lookup (Multiple)
        /// <summary>
        /// Gets all public and non-public instance properties on the given <paramref name="type"/>,
        /// including properties defined on base types. The result can optionally be filtered by specifying
        /// a list of property names to include using the <paramref name="names"/> parameter.
        /// </summary>
        /// <returns>A list of matching instance properties on the type.</returns>
        /// <param name="type">The type whose public properties are to be retrieved.</param>
        /// <param name="names">A list of names of properties to be retrieved. If this is <c>null</c>, 
        /// all properties are returned.</param>
        /// <returns>A list of all public properties on the type filted by <paramref name="names"/>.
        /// This value will never be null.</returns>
        public static IList<PropertyInfo> Properties( this Type type, params string[] names )
        {
            return type.Properties( Flags.InstanceAnyVisibility, names );
        }

        /// <summary>
        /// Gets all properties on the given <paramref name="type"/> that match the specified <paramref name="bindingFlags"/>,
        /// including properties defined on base types.
        /// </summary>
        /// <returns>A list of all matching properties on the type. This value will never be null.</returns>
        public static IList<PropertyInfo> Properties( this Type type, Flags bindingFlags, params string[] names )
        {
            if (type == null || type == Constants.ObjectType)
            {
                return Constants.EmptyPropertyInfoArray;
            }

            bool recurse = bindingFlags.IsNotSet( Flags.DeclaredOnly );
            bool hasNames = names != null && names.Length > 0;
            bool hasSpecialFlags =
                bindingFlags.IsAnySet( Flags.ExcludeBackingMembers | Flags.ExcludeExplicitlyImplemented );

            if( ! recurse && ! hasNames && ! hasSpecialFlags )
            {
                return type.GetProperties( bindingFlags ) ?? Constants.EmptyPropertyInfoArray;
            }

            var properties = GetProperties( type, bindingFlags );
            properties = hasSpecialFlags ? properties.Filter( bindingFlags ) : properties;
            properties = hasNames ? properties.Filter( bindingFlags, names ) : properties;
            return properties;
        }

        private static IList<PropertyInfo> GetProperties( Type type, Flags bindingFlags )
        {
            bool recurse = bindingFlags.IsNotSet( Flags.DeclaredOnly );

            if( ! recurse )
            {
                return type.GetProperties( bindingFlags ) ?? Constants.EmptyPropertyInfoArray;
            }

            bindingFlags |= Flags.DeclaredOnly;
            bindingFlags &= ~BindingFlags.FlattenHierarchy;

            var properties = new List<PropertyInfo>();
            properties.AddRange( type.GetProperties( bindingFlags ) );
            Type baseType = type.BaseType;
            while( baseType != null && baseType != typeof(object) )
            {
                properties.AddRange( baseType.GetProperties( bindingFlags ) );
                baseType = baseType.BaseType;
            }
            return properties;
        }
        #endregion

        #region Property Combined

        #region TryGetValue
		/// <summary>
        /// Gets the first (public or non-public) instance property with the given <paramref name="name"/> on the given
        /// <paramref name="obj"/> object. Returns the value of the property if a match was found and null otherwise.
		/// </summary>
		/// <remarks>
        /// When using this method it is not possible to distinguish between a missing property and a property whose value is null.
		/// </remarks>
		/// <param name="obj">The source object on which to find the property</param>
		/// <param name="name">The name of the property whose value should be retrieved</param>
		/// <returns>The value of the property or null if no property was found</returns>
        public static object TryGetPropertyValue( this object obj, string name )
        {
            return TryGetPropertyValue( obj, name, Flags.InstanceAnyVisibility );
        }

		/// <summary>
        /// Gets the first property with the given <paramref name="name"/> on the given <paramref name="obj"/> object.
        /// Returns the value of the property if a match was found and null otherwise.
        /// Use the <paramref name="bindingFlags"/> parameter to limit the scope of the search.
		/// </summary>
		/// <remarks>
        /// When using this method it is not possible to distinguish between a missing property and a property whose value is null.
		/// </remarks>
		/// <param name="obj">The source object on which to find the property</param>
		/// <param name="name">The name of the property whose value should be retrieved</param>
		/// <param name="bindingFlags">A combination of Flags that define the scope of the search</param>
		/// <returns>The value of the property or null if no property was found</returns>
        public static object TryGetPropertyValue( this object obj, string name, Flags bindingFlags )
        {
            try
            {
                return obj.GetPropertyValue( name, bindingFlags );
            }
            catch( MissingMemberException )
            {
                return null;
            }
        }
        #endregion

        #region TrySetValue
		/// <summary>
        /// Sets the first (public or non-public) instance property with the given <paramref name="name"/> on the 
        /// given <paramref name="obj"/> object to the supplied <paramref name="value"/>. Returns true 
        /// if a value was assigned to a property and false otherwise.
		/// </summary>
		/// <param name="obj">The source object on which to find the property</param>
		/// <param name="name">The name of the property whose value should be retrieved</param>
		/// <param name="value">The value that should be assigned to the property</param>
		/// <returns>True if the value was assigned to a property and false otherwise</returns>
        public static bool TrySetPropertyValue( this object obj, string name, object value )
        {
            return TrySetPropertyValue( obj, name, value, Flags.InstanceAnyVisibility );
        }

		/// <summary>
        /// Sets the first property with the given <paramref name="name"/> on the given <paramref name="obj"/> object
        /// to the supplied <paramref name="value"/>. Returns true if a value was assigned to a property and false otherwise.
        /// Use the <paramref name="bindingFlags"/> parameter to limit the scope of the search.
		/// </summary>
		/// <param name="obj">The source object on which to find the property</param>
		/// <param name="name">The name of the property whose value should be retrieved</param>
		/// <param name="value">The value that should be assigned to the property</param>
		/// <param name="bindingFlags">A combination of Flags that define the scope of the search</param>
		/// <returns>True if the value was assigned to a property and false otherwise</returns>
        public static bool TrySetPropertyValue( this object obj, string name, object value, Flags bindingFlags )
        {
            try
            {
                obj.SetPropertyValue( name, value, bindingFlags );
                return true;
            }
            catch (MissingMemberException)
            {
                return false;
            }
        }
        #endregion

        #endregion
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Core\PropertyInfoExtensions.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect
{
    using System.Reflection;
    using Fasterflect.Emitter;

	/// <summary>
    /// Extension methods for inspecting and working with properties.
    /// </summary>
    internal static class PropertyInfoExtensions
    {
        /// <summary>
        /// Sets the static property identified by <paramref name="propInfo"/> to the specified <paramref name="value" />.
        /// </summary>
        public static void Set( this PropertyInfo propInfo, object value )
        {
            propInfo.DelegateForSetPropertyValue( Flags.StaticAnyVisibility )( null, value );
        }

        /// <summary>
        /// Sets the instance property identified by <paramref name="propInfo"/> on the given <paramref name="obj"/>
        /// to the specified <paramref name="value" />.
        /// </summary>
        public static void Set( this PropertyInfo propInfo, object obj, object value )
        {
            propInfo.DelegateForSetPropertyValue( Flags.InstanceAnyVisibility )( obj, value );
        }

        /// <summary>
        /// Gets the value of the static property identified by <paramref name="propInfo"/>.
        /// </summary>
        public static object Get( this PropertyInfo propInfo )
        {
            return propInfo.DelegateForGetPropertyValue( Flags.StaticAnyVisibility )( null );
        }

        /// <summary>
        /// Gets the value of the instance property identified by <paramref name="propInfo"/> on the given <paramref name="obj"/>.
        /// </summary>
        public static object Get( this PropertyInfo propInfo, object obj )
        {
            return propInfo.DelegateForGetPropertyValue( Flags.InstanceAnyVisibility )( obj );
        }

        /// <summary>
        /// Creates a delegate which can set the value of the property <paramref name="propInfo"/>.
        /// </summary>
		public static MemberSetter DelegateForSetPropertyValue( this PropertyInfo propInfo )
		{
		    var flags = propInfo.IsStatic() ? Flags.StaticAnyVisibility : Flags.InstanceAnyVisibility;
		    return (MemberSetter) new MemberSetEmitter( propInfo, flags ).GetDelegate();
		}

		/// <summary>
        /// Creates a delegate which can set the value of the property <param name="propInfo"/> matching the
        /// specified <param name="bindingFlags" />.
        /// </summary>
        public static MemberSetter DelegateForSetPropertyValue( this PropertyInfo propInfo, Flags bindingFlags )
        {
            return (MemberSetter) new MemberSetEmitter( propInfo, bindingFlags ).GetDelegate();
        }

        /// <summary>
        /// Creates a delegate which can get the value of the property <param name="propInfo"/>.
        /// </summary>
		public static MemberGetter DelegateForGetPropertyValue( this PropertyInfo propInfo )
		{
		    var flags = propInfo.IsStatic() ? Flags.StaticAnyVisibility : Flags.InstanceAnyVisibility;
		    return (MemberGetter) new MemberGetEmitter( propInfo, flags ).GetDelegate();
		}

        /// <summary>
        /// Creates a delegate which can get the value of the property <param name="propInfo"/> matching the
        /// specified <param name="bindingFlags" />.
        /// </summary>
        public static MemberGetter DelegateForGetPropertyValue( this PropertyInfo propInfo, Flags bindingFlags )
        {
            return (MemberGetter) new MemberGetEmitter( propInfo, bindingFlags ).GetDelegate();
        }
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Core\TypeExtensions.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

	/// <summary>
	/// Extension methods for inspecting types.
	/// </summary>
	internal static class TypeExtensions
	{
		#region Implements
		/// <summary>
		/// Returns true of the supplied <paramref name="type"/> implements the given <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The type (interface) to check for.</typeparam>
		/// <param name="type">The type to check.</param>
		/// <returns>True if the given type implements the interface.</returns>
		public static bool Implements<T>( this Type type )
		{
			return typeof(T).IsAssignableFrom( type ) && typeof(T) != type;
		}
		#endregion

		#region IsFrameworkType
		#region IsFrameworkType Helpers
		private static readonly List<byte[]> tokens = new List<byte[]>
		                                              {
		                                              	new byte[] { 0xb7, 0x7a, 0x5c, 0x56, 0x19, 0x34, 0xe0, 0x89 },
		                                              	new byte[] { 0x31, 0xbf, 0x38, 0x56, 0xad, 0x36, 0x4e, 0x35 },
		                                              	new byte[] { 0xb0, 0x3f, 0x5f, 0x7f, 0x11, 0xd5, 0x0a, 0x3a }
		                                              };

		internal class ByteArrayEqualityComparer : EqualityComparer<byte[]>
		{
			public override bool Equals( byte[] x, byte[] y )
			{
				return x != null && y != null && x.SequenceEqual( y );
			}

			public override int GetHashCode( byte[] obj )
			{
				return obj.GetHashCode();
			}
		}
		#endregion

		/// <summary>
		/// Returns true if the supplied type is defined in an assembly signed by Microsoft.
		/// </summary>
		public static bool IsFrameworkType( this Type type )
		{
			if( type == null )
			{
				throw new ArgumentNullException( "type" );
			}
			byte[] publicKeyToken = type.Assembly.GetName().GetPublicKeyToken();
			return publicKeyToken != null && tokens.Contains( publicKeyToken, new ByteArrayEqualityComparer() );
		}
		#endregion
	}
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Core\ValueTypeExtensions.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect
{
    using Fasterflect.Emitter;

    /// <summary>
    /// Extension methods for working with types.
    /// </summary>
    internal static class ValueTypeExtensions
    {
        ///<summary>
        /// Returns a wrapper <see cref="ValueTypeHolder"/> instance if <paramref name="obj"/> 
        /// is a value type.  Otherwise, returns <paramref name="obj"/>.
        ///</summary>
        ///<param name="obj">An object to be examined.</param>
        ///<returns>A wrapper <seealso cref="ValueTypeHolder"/> instance if <paramref name="obj"/>
        /// is a value type, or <paramref name="obj"/> itself if it's a reference type.</returns>
        public static object WrapIfValueType( this object obj )
        {
            return obj.GetType().IsValueType ? new ValueTypeHolder( obj ) : obj;
        }

        ///<summary>
        /// Returns a wrapped object if <paramref name="obj"/> is an instance of <see cref="ValueTypeHolder"/>.
        ///</summary>
        ///<param name="obj">An object to be "erased".</param>
        ///<returns>The object wrapped by <paramref name="obj"/> if the latter is of type <see cref="ValueTypeHolder"/>.  Otherwise,
        /// return <paramref name="obj"/>.</returns>
        public static object UnwrapIfWrapped( this object obj )
        {
            var holder = obj as ValueTypeHolder;
            return holder == null ? obj : holder.Value;
        }

        /// <summary>
        /// Determines whether <paramref name="obj"/> is a wrapped object (instance of <see cref="ValueTypeHolder"/>).
        /// </summary>
        /// <param name="obj">The object to check.</param>
        /// <returns>Returns true if <paramref name="obj"/> is a wrapped object (instance of <see cref="ValueTypeHolder"/>).</returns>
        public static bool IsWrapped( this object obj )
        {
            return obj as ValueTypeHolder != null;
        }
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Services\CloneExtensions.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Extension methods for deep cloning of objects.
    /// </summary>
    internal static class CloneExtensions
    {
        /// <summary>
        /// Produces a deep clone of the <paramref name="source"/> object. Reference integrity is maintained and
        /// every unique object in the graph is cloned only once.
        /// A current limitation of this method is that all objects in the graph must have a default constructor.
        /// </summary>
        /// <typeparam name="T">The type of the object to clone.</typeparam>
        /// <param name="source">The object to clone.</param>
        /// <returns>A deep clone of the source object.</returns>
        public static T DeepClone<T>( this T source ) where T : class, new()
        {
            return source.DeepClone( null );
        }

        #region Private Helpers
        private static T DeepClone<T>( this T source, Dictionary<object, object> map ) where T : class, new()
        {
            Type type = source.GetType();
            var clone = type.IsArray ? Activator.CreateInstance( type, ((ICollection) source).Count ) as T : type.CreateInstance() as T;
            map = map ?? new Dictionary<object, object>();
            map[ source ] = clone;
			if( type.IsArray )
			{
				source.CloneArray( clone, map );
				return clone;
			}
        	IList<FieldInfo> fields = type.Fields( Flags.StaticInstanceAnyVisibility ).Where( f => ! f.IsLiteral ).ToList();
            object[] values = fields.Select( f => GetValue( f, source, map ) ).ToArray();
            for( int i = 0; i < fields.Count; i++ )
            {
				fields[ i ].Set( clone, values[ i ] );
            }
            return clone;
        }
        private static void CloneArray<T>( this T source, T clone, Dictionary<object, object> map ) where T : class, new()
        {
        	var sourceList = (IList) source;
        	var cloneList = (IList) clone;
			for( int i=0; i<sourceList.Count; i++ )
			{
				object element = sourceList[ i ];
            	cloneList[ i ] = element.ShouldClone() ? element.DeepClone( map ) : element;
			}
        }

        private static object GetValue( FieldInfo field, object source, Dictionary<object, object> map )
        {
            object result = field.IsLiteral ? field.GetRawConstantValue() : field.Get( source );
			if( result == null || ! result.ShouldClone() )
			{
				return result;
			}
			object clone;
            if( map.TryGetValue( result, out clone ) )
            {
                return clone;
            }
            return result.DeepClone( map );
        }

        private static bool ShouldClone( this object obj )
        {
			if( obj == null )
				return false;
        	Type type = obj.GetType();
			if( type.IsValueType || type == typeof(string) )
				return false;
			if( type.IsGenericTypeDefinition || obj is Type || obj is Delegate )
				return false;
			return true;
		}
        #endregion
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Services\EventExtensions.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect
{
    using System.Reflection;

    using System;
    using System.Linq;
    using System.Linq.Expressions;

    internal static class DynamicHandler
    {
        /// <summary>
        /// Invokes a static delegate using supplied parameters.
        /// </summary>
        /// <param name="targetType">The type where the delegate belongs to.</param>
        /// <param name="delegateName">The field name of the delegate.</param>
        /// <param name="parameters">The parameters used to invoke the delegate.</param>
        /// <returns>The return value of the invocation.</returns>
        public static object InvokeDelegate(this Type targetType, string delegateName, params object[] parameters)
        {
            return ((Delegate)targetType.GetFieldValue(delegateName)).DynamicInvoke(parameters);
        }

        /// <summary>
        /// Invokes an instance delegate using supplied parameters.
        /// </summary>
        /// <param name="target">The object where the delegate belongs to.</param>
        /// <param name="delegateName">The field name of the delegate.</param>
        /// <param name="parameters">The parameters used to invoke the delegate.</param>
        /// <returns>The return value of the invocation.</returns>
        public static object InvokeDelegate(this object target, string delegateName, params object[] parameters)
        {
            return ((Delegate)target.GetFieldValue(delegateName)).DynamicInvoke(parameters);
        }

        /// <summary>
        /// Adds a dynamic handler for a static delegate.
        /// </summary>
        /// <param name="targetType">The type where the delegate belongs to.</param>
        /// <param name="fieldName">The field name of the delegate.</param>
        /// <param name="func">The function which will be invoked whenever the delegate is invoked.</param>
        /// <returns>The return value of the invocation.</returns>
        public static Type AddHandler(this Type targetType, string fieldName,
            Func<object[], object> func)
        {
            return InternalAddHandler(targetType, fieldName, func, null, false);
        }

        /// <summary>
        /// Adds a dynamic handler for an instance delegate.
        /// </summary>
        /// <param name="target">The object where the delegate belongs to.</param>
        /// <param name="fieldName">The field name of the delegate.</param>
        /// <param name="func">The function which will be invoked whenever the delegate is invoked.</param>
        /// <returns>The return value of the invocation.</returns>
        public static Type AddHandler(this object target, string fieldName,
            Func<object[], object> func)
        {
            return InternalAddHandler(target.GetType(), fieldName, func, target, false);
        }

        /// <summary>
        /// Assigns a dynamic handler for a static delegate or event.
        /// </summary>
        /// <param name="targetType">The type where the delegate or event belongs to.</param>
        /// <param name="fieldName">The field name of the delegate or event.</param>
        /// <param name="func">The function which will be invoked whenever the delegate or event is fired.</param>
        /// <returns>The return value of the invocation.</returns>
        public static Type AssignHandler(this Type targetType, string fieldName,
            Func<object[], object> func)
        {
            return InternalAddHandler(targetType, fieldName, func, null, true);
        }

        /// <summary>
        /// Assigns a dynamic handler for a static delegate or event.
        /// </summary>
        /// <param name="target">The object where the delegate or event belongs to.</param>
        /// <param name="fieldName">The field name of the delegate or event.</param>
        /// <param name="func">The function which will be invoked whenever the delegate or event is fired.</param>
        /// <returns>The return value of the invocation.</returns>
        public static Type AssignHandler(this object target, string fieldName,
            Func<object[], object> func)
        {
            return InternalAddHandler(target.GetType(), fieldName, func, target, true);
        }

        private static Type InternalAddHandler(Type targetType, string fieldName,
            Func<object[], object> func, object target, bool assignHandler)
        {
            Type delegateType;
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic |
                               (target == null ? BindingFlags.Static : BindingFlags.Instance);
            var eventInfo = targetType.GetEvent(fieldName, bindingFlags);
            if (eventInfo != null && assignHandler)
                throw new ArgumentException("Event can be assigned.  Use AddHandler() overloads instead.");

            if (eventInfo != null)
            {
                delegateType = eventInfo.EventHandlerType;
                var dynamicHandler = BuildDynamicHandler(delegateType, func);
                eventInfo.GetAddMethod(true).Invoke(target, new Object[] { dynamicHandler });
            }
            else
            {
                var fieldInfo = targetType.Field(fieldName,
                                                    target == null
                                                        ? Flags.StaticAnyVisibility
                                                        : Flags.InstanceAnyVisibility);
                delegateType = fieldInfo.FieldType;
                var dynamicHandler = BuildDynamicHandler(delegateType, func);
                var field = assignHandler ? null : target == null
                                ? (Delegate)fieldInfo.Get()
                                : (Delegate)fieldInfo.Get(target);
                field = field == null
                            ? dynamicHandler
                            : Delegate.Combine(field, dynamicHandler);
                (target ?? targetType).SetFieldValue(fieldName, field);
            }
            return delegateType;
        }

        /// <summary>
        /// Dynamically generates code for a method whose can be used to handle a delegate of type 
        /// <paramref name="delegateType"/>.  The generated method will forward the call to the
        /// supplied <paramref name="func"/>.
        /// </summary>
        /// <param name="delegateType">The delegate type whose dynamic handler is to be built.</param>
        /// <param name="func">The function which will be forwarded the call whenever the generated
        /// handler is invoked.</param>
        /// <returns></returns>
        public static Delegate BuildDynamicHandler(this Type delegateType, Func<object[], object> func)
        {
            var invokeMethod = delegateType.GetMethod("Invoke");
            var parameters = invokeMethod.GetParameters().Select(parm =>
                Expression.Parameter(parm.ParameterType, parm.Name)).ToArray();
            var instance = func.Target == null ? null : Expression.Constant(func.Target);
            var convertedParameters = parameters.Select(parm => Expression.Convert(parm, typeof(object)));
            var call = Expression.Call(instance, func.Method, Expression.NewArrayInit(typeof(object), convertedParameters));
            var body = invokeMethod.ReturnType == typeof(void)
                ? (Expression)call
                : Expression.Convert(call, invokeMethod.ReturnType);
            var expr = Expression.Lambda(delegateType, body, parameters);
            return expr.Compile();
        }
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Services\MapExtensions.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect
{
    using System;
    using System.Reflection;
    using Fasterflect.Emitter;

    /// <summary>
    /// Extension methods for mapping (copying) members from one object instance to another.
    /// </summary>
    internal static class MapExtensions
    {
		#region Map
		/// <summary>
		/// Maps values from fields and properties on the source object to fields and properties with the 
		/// same name on the target object.
		/// </summary>
		/// <param name="source">The source object from which member values are read.</param>
		/// <param name="target">The target object to which member values are written.</param>
        /// <param name="names">The optional list of member names against which to filter the members that are
        /// to be mapped. If this parameter is <c>null</c> or empty no name filtering will be applied. The default 
        /// behavior is to check for an exact, case-sensitive match. Pass <see href="Flags.PartialNameMatch"/> to 
        /// filter members by substring and <see href="Flags.IgnoreCase"/> to ignore case.</param>
		public static void Map( this object source, object target, params string[] names )
		{
            DelegateForMap(source.GetType(), target.GetTypeAdjusted(), names)(source, target);
		}

		/// <summary>
		/// Maps values from fields and properties on the source object to fields and properties with the 
		/// same name on the target object.
		/// </summary>
		/// <param name="source">The source object from which member values are read.</param>
		/// <param name="target">The target object to which member values are written.</param>
		/// <param name="bindingFlags">The <see href="Flags"/> used to define the scope when locating members.</param>
        /// <param name="names">The optional list of member names against which to filter the members that are
        /// to be mapped. If this parameter is <c>null</c> or empty no name filtering will be applied. The default 
        /// behavior is to check for an exact, case-sensitive match. Pass <see href="Flags.PartialNameMatch"/> to 
        /// filter members by substring and <see href="Flags.IgnoreCase"/> to ignore case.</param>
		public static void Map( this object source, object target, Flags bindingFlags, params string[] names )
		{
            DelegateForMap(source.GetType(), target.GetTypeAdjusted(), bindingFlags, names)(source, target);
		}

		/// <summary>
		/// Maps values from members on the source object to members with the same name on the target object.
		/// </summary>
		/// <param name="source">The source object from which member values are read.</param>
		/// <param name="target">The target object to which member values are written.</param>
		/// <param name="sourceTypes">The member types (Fields, Properties or both) to include on the source.</param>
		/// <param name="targetTypes">The member types (Fields, Properties or both) to include on the target.</param>
		/// <param name="bindingFlags">The <see href="Flags"/> used to define the scope when locating members. If
		/// <paramref name="sourceTypes"/> is different from <paramref name="targetTypes"/> the flag value
		/// <see cref="Flags.IgnoreCase"/> will automatically be applied.</param>
        /// <param name="names">The optional list of member names against which to filter the members that are
        /// to be mapped. If this parameter is <c>null</c> or empty no name filtering will be applied. The default 
        /// behavior is to check for an exact, case-sensitive match. Pass <see href="Flags.PartialNameMatch"/> to 
        /// filter members by substring and <see href="Flags.IgnoreCase"/> to ignore case.</param>
		public static void Map( this object source, object target, MemberTypes sourceTypes, MemberTypes targetTypes, 
								Flags bindingFlags, params string[] names )
		{
		    DelegateForMap( source.GetType(), target.GetTypeAdjusted(), sourceTypes, targetTypes, bindingFlags, names )(
		        source, target );
		}

        /// <summary>
        /// Creates a delegate that can map values from fields and properties on the source object to fields and properties with the 
        /// same name on the target object.
        /// </summary>
        /// <param name="sourceType">The type of the source object.</param>
        /// <param name="targetType">The type of the target object.</param>
        /// <param name="names">The optional list of member names against which to filter the members that are
        /// to be mapped. If this parameter is <c>null</c> or empty no name filtering will be applied. The default 
        /// behavior is to check for an exact, case-sensitive match. Pass <see href="Flags.PartialNameMatch"/> to 
        /// filter members by substring and <see href="Flags.IgnoreCase"/> to ignore case.</param>
        public static ObjectMapper DelegateForMap(this Type sourceType, Type targetType, params string[] names)
        {
            return DelegateForMap(sourceType, targetType, Flags.InstanceAnyVisibility, names);
        }

        /// <summary>
        /// Creates a delegate that can map values from fields and properties on the source object to fields and properties with the 
        /// same name on the target object.
        /// </summary>
        /// <param name="sourceType">The type of the source object.</param>
        /// <param name="targetType">The type of the target object.</param>
		/// <param name="bindingFlags">The <see href="Flags"/> used to define the scope when locating members.</param>
        /// <param name="names">The optional list of member names against which to filter the members that are
        /// to be mapped. If this parameter is <c>null</c> or empty no name filtering will be applied. The default 
        /// behavior is to check for an exact, case-sensitive match. Pass <see href="Flags.PartialNameMatch"/> to 
        /// filter members by substring and <see href="Flags.IgnoreCase"/> to ignore case.</param>
        public static ObjectMapper DelegateForMap(this Type sourceType, Type targetType, Flags bindingFlags, params string[] names)
        {
            const MemberTypes memberTypes = MemberTypes.Field | MemberTypes.Property;
            return DelegateForMap( sourceType, targetType, memberTypes, memberTypes, bindingFlags, names );
        }

        /// <summary>
        /// Creates a delegate that can map values from fields and properties on the source object to fields and properties with the 
        /// same name on the target object.
        /// </summary>
        /// <param name="sourceType">The type of the source object.</param>
        /// <param name="targetType">The type of the target object.</param>
        /// <param name="sourceTypes">The member types (Fields, Properties or both) to include on the source.</param>
        /// <param name="targetTypes">The member types (Fields, Properties or both) to include on the target.</param>
        /// <param name="bindingFlags">The <see href="Flags"/> used to define the scope when locating members. If
        /// <paramref name="sourceTypes"/> is different from <paramref name="targetTypes"/> the flag value
        /// <see cref="Flags.IgnoreCase"/> will automatically be applied.</param>
        /// <param name="names">The optional list of member names against which to filter the members that are
        /// to be mapped. If this parameter is <c>null</c> or empty no name filtering will be applied. The default 
        /// behavior is to check for an exact, case-sensitive match. Pass <see href="Flags.PartialNameMatch"/> to 
        /// filter members by substring and <see href="Flags.IgnoreCase"/> to ignore case.</param>
        public static ObjectMapper DelegateForMap(this Type sourceType, Type targetType, MemberTypes sourceTypes, MemberTypes targetTypes,
                               Flags bindingFlags, params string[] names)
        {
            var emitter = new MapEmitter(sourceType, targetType, sourceTypes, targetTypes, bindingFlags, names);
            return (ObjectMapper)emitter.GetDelegate();
        }
    	#endregion

		#region Map Companions
		/// <summary>
		/// Maps values from fields on the source object to fields with the same name on the target object.
		/// </summary>
		/// <param name="source">The source object from which field values are read.</param>
		/// <param name="target">The target object to which field values are written.</param>
        /// <param name="names">The optional list of field names against which to filter the fields that are
        /// to be mapped. If this parameter is <c>null</c> or empty no name filtering will be applied. The default 
        /// behavior is to check for an exact, case-sensitive match. Pass <see href="Flags.PartialNameMatch"/> to 
        /// filter fields by substring and <see href="Flags.IgnoreCase"/> to ignore case.</param>
    	public static void MapFields( this object source, object target, params string[] names )
		{
			source.Map( target, MemberTypes.Field, MemberTypes.Field, Flags.InstanceAnyVisibility, names );
		}

		/// <summary>
		/// Maps values from properties on the source object to properties with the same name on the target object.
		/// </summary>
		/// <param name="source">The source object from which property values are read.</param>
		/// <param name="target">The target object to which property values are written.</param>
        /// <param name="names">The optional list of property names against which to filter the properties that are
        /// to be mapped. If this parameter is <c>null</c> or empty no name filtering will be applied. The default 
        /// behavior is to check for an exact, case-sensitive match. Pass <see href="Flags.PartialNameMatch"/> to 
        /// filter properties by substring and <see href="Flags.IgnoreCase"/> to ignore case.</param>
		public static void MapProperties( this object source, object target, params string[] names )
		{
			source.Map( target, MemberTypes.Property, MemberTypes.Property, Flags.InstanceAnyVisibility, names );
		}

		/// <summary>
		/// Maps values from fields on the source object to properties with the same name (ignoring case)
		/// on the target object.
		/// </summary>
		/// <param name="source">The source object from which field values are read.</param>
		/// <param name="target">The target object to which property values are written.</param>
        /// <param name="names">The optional list of member names against which to filter the members that are
        /// to be mapped. If this parameter is <c>null</c> or empty no name filtering will be applied. The default 
        /// behavior is to check for an exact, case-insensitive match. Pass <see href="Flags.PartialNameMatch"/>
        /// to filter members by substring.</param>
		public static void MapFieldsToProperties( this object source, object target, params string[] names )
		{
			source.Map( target, MemberTypes.Field, MemberTypes.Property, Flags.InstanceAnyVisibility, names );
		}

		/// <summary>
		/// Maps values from properties on the source object to fields with the same name (ignoring case) 
		/// on the target object.
		/// </summary>
		/// <param name="source">The source object from which property values are read.</param>
		/// <param name="target">The target object to which field values are written.</param>
        /// <param name="names">The optional list of member names against which to filter the members that are
        /// to be mapped. If this parameter is <c>null</c> or empty no name filtering will be applied. The default 
        /// behavior is to check for an exact, case-insensitive match. Pass <see href="Flags.PartialNameMatch"/>
        /// to filter members by substring.</param>
		public static void MapPropertiesToFields( this object source, object target, params string[] names )
		{
			source.Map( target, MemberTypes.Property, MemberTypes.Field, Flags.InstanceAnyVisibility, names );
		}
    	#endregion
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Services\TryCallMethodExtensions.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Fasterflect.Probing;

    /// <summary>
    /// Extension methods for creating object instances when you do not know which constructor to call.
    /// </summary>
    internal static class TryCallMethodExtensions
    {
        #region Method Invocation (TryCallMethod)
        /// <summary>
        /// Obtains a list of all methods with the given <paramref name="methodName"/> on the given 
        /// <paramref name="obj" />, and invokes the best match for the parameters obtained from the 
        /// public properties of the supplied <paramref name="sample"/> object.
        /// TryCallMethod is very liberal and attempts to convert values that are not otherwise
        /// considered compatible, such as between strings and enums or numbers, Guids and byte[16], etc.
        /// </summary>
        /// <returns>The result of the invocation.</returns>
        public static object TryCallMethod( this object obj, string methodName, bool mustUseAllParameters, object sample )
        {
            Type sourceType = sample.GetType();
            var sourceInfo = new SourceInfo( sourceType );
            var paramValues = sourceInfo.GetParameterValues( sample );
			return obj.TryCallMethod( methodName, mustUseAllParameters, sourceInfo.ParamNames, sourceInfo.ParamTypes, paramValues );
        }

        /// <summary>
        /// Obtains a list of all methods with the given <paramref name="methodName"/> on the given 
        /// <paramref name="obj" />, and invokes the best match for the parameters obtained from the 
        /// values in the supplied <paramref name="parameters"/> dictionary.
        /// TryCallMethod is very liberal and attempts to convert values that are not otherwise
        /// considered compatible, such as between strings and enums or numbers, Guids and byte[16], etc.
        /// </summary>
        /// <returns>The result of the invocation.</returns>
        public static object TryCallMethod( this object obj, string methodName, bool mustUseAllParameters, IDictionary<string, object> parameters )
        {
			bool hasParameters = parameters != null && parameters.Count > 0;
            string[] names = hasParameters ? parameters.Keys.ToArray() : new string[ 0 ];
            object[] values = hasParameters ? parameters.Values.ToArray() : new object[ 0 ];
            return obj.TryCallMethod( methodName, mustUseAllParameters, names, values.ToTypeArray(), values );
        }

        /// <summary>
        /// Obtains a list of all methods with the given <paramref name="methodName"/> on the given 
        /// <paramref name="obj" />, and invokes the best match for the supplied parameters.
        /// TryCallMethod is very liberal and attempts to convert values that are not otherwise
        /// considered compatible, such as between strings and enums or numbers, Guids and byte[16], etc.
        /// </summary>
        /// <param name="obj">The type of which an instance should be created.</param>
        /// <param name="methodName">The name of the overloaded methods.</param>
		/// <param name="mustUseAllParameters">Specifies whether all supplied parameters must be used in the
		/// invocation. Unless you know what you are doing you should pass true for this parameter.</param>
        /// <param name="parameterNames">The names of the supplied parameters.</param>
        /// <param name="parameterTypes">The types of the supplied parameters.</param>
        /// <param name="parameterValues">The values of the supplied parameters.</param>
        /// <returns>The result of the invocation.</returns>
        public static object TryCallMethod( this object obj, string methodName, bool mustUseAllParameters, 
											string[] parameterNames, Type[] parameterTypes, object[] parameterValues )
        {
        	bool isStatic = obj is Type;
			var type = isStatic ? obj as Type : obj.GetType();
        	var names = parameterNames ?? new string[ 0 ];
        	var types = parameterTypes ?? new Type[ 0 ];
			var values = parameterValues ?? new object[ 0 ];
			if( names.Length != values.Length || names.Length != types.Length )
			{
                throw new ArgumentException( "Mismatching name, type and value arrays (must be of identical length)." );
			}
			MethodMap map = MapFactory.DetermineBestMethodMatch( type.Methods( methodName ).Cast<MethodBase>(), mustUseAllParameters, names, types, values );
			return isStatic ? map.Invoke( values ) : map.Invoke( obj, values );
		}
        #endregion
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Services\TryCreateInstanceExtensions.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Fasterflect.Caching;
    using Fasterflect.Probing;

    /// <summary>
    /// Extension methods for creating object instances when you do not know which constructor to call.
    /// </summary>
    internal static class TryCreateInstanceExtensions
    {
        /// <summary>
        /// This field is used to cache information on objects used as parameters for object construction, which
        /// improves performance for subsequent instantiations of the same type using a compatible source type.
        /// </summary>
        private static readonly Cache<Type, SourceInfo> sourceInfoCache = new Cache<Type, SourceInfo>();

        #region Constructor Invocation (TryCreateInstance)
        /// <summary>
        /// Creates an instance of the given <paramref name="type"/> using the public properties of the 
        /// supplied <paramref name="sample"/> object as input.
        /// This method will try to determine the least-cost route to constructing the instance, which
        /// implies mapping as many properties as possible to constructor parameters. Remaining properties
        /// on the source are mapped to properties on the created instance or ignored if none matches.
        /// TryCreateInstance is very liberal and attempts to convert values that are not otherwise
        /// considered compatible, such as between strings and enums or numbers, Guids and byte[], etc.
        /// </summary>
        /// <returns>An instance of <paramref name="type"/>.</returns>
        public static object TryCreateInstance( this Type type, object sample )
        {
            Type sourceType = sample.GetType();
            SourceInfo sourceInfo = sourceInfoCache.Get( sourceType );
            if( sourceInfo == null )
            {
                sourceInfo = new SourceInfo( sourceType );
                sourceInfoCache.Insert( sourceType, sourceInfo );
            }
            object[] paramValues = sourceInfo.GetParameterValues( sample );
            MethodMap map = MapFactory.PrepareInvoke( type, sourceInfo.ParamNames, sourceInfo.ParamTypes, paramValues );
            return map.Invoke( paramValues );
        }

        /// <summary>
        /// Creates an instance of the given <paramref name="type"/> using the values in the supplied
        /// <paramref name="parameters"/> dictionary as input.
        /// This method will try to determine the least-cost route to constructing the instance, which
        /// implies mapping as many values as possible to constructor parameters. Remaining values
        /// are mapped to properties on the created instance or ignored if none matches.
        /// TryCreateInstance is very liberal and attempts to convert values that are not otherwise
        /// considered compatible, such as between strings and enums or numbers, Guids and byte[], etc.
        /// </summary>
        /// <returns>An instance of <paramref name="type"/>.</returns>
        public static object TryCreateInstance( this Type type, IDictionary<string, object> parameters )
        {
			bool hasParameters = parameters != null && parameters.Count > 0;
            string[] names = hasParameters ? parameters.Keys.ToArray() : new string[ 0 ];
            object[] values = hasParameters ? parameters.Values.ToArray() : new object[ 0 ];
            return type.TryCreateInstance( names, values );
        }

        /// <summary>
        /// Creates an instance of the given <paramref name="type"/> using the supplied parameter information as input.
        /// Parameter types are inferred from the supplied <paramref name="parameterValues"/> and as such these
        /// should not be null.
        /// This method will try to determine the least-cost route to constructing the instance, which
        /// implies mapping as many properties as possible to constructor parameters. Remaining properties
        /// on the source are mapped to properties on the created instance or ignored if none matches.
        /// TryCreateInstance is very liberal and attempts to convert values that are not otherwise
        /// considered compatible, such as between strings and enums or numbers, Guids and byte[], etc.
        /// </summary>
        /// <param name="type">The type of which an instance should be created.</param>
        /// <param name="parameterNames">The names of the supplied parameters.</param>
        /// <param name="parameterValues">The values of the supplied parameters.</param>
        /// <returns>An instance of <paramref name="type"/>.</returns>
        public static object TryCreateInstance( this Type type, string[] parameterNames, object[] parameterValues )
        {
        	var names = parameterNames ?? new string[ 0 ];
			var values = parameterValues ?? new object[ 0 ];
			if( names.Length != values.Length )
			{
				throw new ArgumentException( "Mismatching name and value arrays (must be of identical length)." );
			}
            var parameterTypes = new Type[ names.Length ];
			for( int i = 0; i < names.Length; i++ )
            {
                object value = values[ i ];
                parameterTypes[ i ] = value != null ? value.GetType() : null;
            }
            return type.TryCreateInstance( names, parameterTypes, values );
        }

        /// <summary>
        /// Creates an instance of the given <paramref name="type"/> using the supplied parameter information as input.
        /// This method will try to determine the least-cost route to constructing the instance, which
        /// implies mapping as many properties as possible to constructor parameters. Remaining properties
        /// on the source are mapped to properties on the created instance or ignored if none matches.
        /// TryCreateInstance is very liberal and attempts to convert values that are not otherwise
        /// considered compatible, such as between strings and enums or numbers, Guids and byte[], etc.
        /// </summary>
        /// <param name="type">The type of which an instance should be created.</param>
        /// <param name="parameterNames">The names of the supplied parameters.</param>
        /// <param name="parameterTypes">The types of the supplied parameters.</param>
        /// <param name="parameterValues">The values of the supplied parameters.</param>
        /// <returns>An instance of <paramref name="type"/>.</returns>
        public static object TryCreateInstance( this Type type, string[] parameterNames, Type[] parameterTypes,
                                                object[] parameterValues )
        {
        	var names = parameterNames ?? new string[ 0 ];
        	var types = parameterTypes ?? new Type[ 0 ];
			var values = parameterValues ?? new object[ 0 ];
			if( names.Length != values.Length || names.Length != types.Length )
			{
                throw new ArgumentException( "Mismatching name, type and value arrays (must be of identical length)." );
			}
            MethodMap map = MapFactory.PrepareInvoke( type, names, types, values );
            return map.Invoke( values );
        }

        #endregion
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Services\TryInvokeWithValuesExtensions.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Fasterflect.Probing;

    /// <summary>
    /// A converter used to convert <paramref name="value"/> to <paramref name="parameterType"/>
    /// if it makes sense in the application.  Why implementation of converter can
    /// set new value for <paramref name="value"/>, it should not attempt to 
    /// modify child objects of <paramref name="value"/> because those changes will
    /// be permanent although if the method in question will not be selected as a match.
    /// </summary>
    /// <param name="parameterType">The type to be converted to.</param>
    /// <param name="target">The type or object whose method or constructor is being called.</param>
    /// <param name="value">The value to be converted.</param>
    /// <returns></returns>
    internal delegate bool ParameterConverter(Type parameterType, object target, ref object value);

    internal static class TryInvokeWithValuesExtensions
    {
        /// <summary>
        /// Obtains the list of contructors for <paramref name="type"/> using the supplied parameter values
        /// and invokes the best match. This overload requires that the supplied <paramref name="parameterValues"/> 
        /// are all used in the order in which they are supplied. Parameter values can be null.
        /// 
        /// This method is very liberal and attempts to convert values that are not otherwise
        /// considered compatible, such as between strings and enums or numbers, Guids and byte[16], etc.
        /// You should carefully test any usage to ensure correct program behavior.
        /// </summary>
        /// <param name="type">The type of which an instance should be created.</param>
        /// <param name="parameterValues">The values to use when invoking the constructor.</param>
        /// <returns>The result of the invocation.</returns>
        public static object TryCreateInstanceWithValues(this Type type, params object[] parameterValues)
        {
            return TryCreateInstanceWithValues(type, null, Flags.InstanceAnyVisibility, parameterValues);
        }

        /// <summary>
        /// Obtains the list of contructors for <paramref name="type"/> using the supplied parameter values
        /// and invokes the best match. This overload requires that the supplied <paramref name="parameterValues"/> 
        /// are all used in the order in which they are supplied. Parameter values can be null.
        /// 
        /// This method is very liberal and attempts to convert values that are not otherwise
        /// considered compatible, such as between strings and enums or numbers, Guids and byte[16], etc.
        /// You should carefully test any usage to ensure correct program behavior.
        /// 
        /// If the default conversion rule doesn't do what you want, you can supply a custom converter.
        /// If it is null, default conversion rule is used.
        /// </summary>
        /// <param name="type">The type of which an instance should be created.</param>
        /// <param name="converter">The converter delegate used to perform user-defined conversion.</param>
        /// <param name="flags">Binding flags for look up constructors.</param>
        /// <param name="parameterValues">The values to use when invoking the constructor.</param>
        /// <returns>The result of the invocation.</returns>
        public static object TryCreateInstanceWithValues(this Type type, ParameterConverter converter, BindingFlags flags, params object[] parameterValues)
        {
            var ctors = type.Constructors();
            try
            {
                return TryCall(converter, ctors, type, parameterValues);
            }
            catch (MissingMemberException)
            {
                var values = parameterValues ?? new object[0];
                throw new MissingMemberException(string.Format("Unable to locate a matching constructor on type {0} for parameters: {1}",
                                                                 type.Name,
                                                                 string.Join(", ", values.Select(v => v == null ? "null" : v.ToString()))));
            }
        }

        /// <summary>
        /// Obtains the list of methods for <paramref name="obj"/> using the supplied parameter values
        /// and invokes the best match. This overload requires that the supplied <paramref name="parameterValues"/> 
        /// are all used in the order in which they are supplied. Parameter values can be null.
        /// 
        /// This method is very liberal and attempts to convert values that are not otherwise
        /// considered compatible, such as between strings and enums or numbers, Guids and byte[16], etc.
        /// You should carefully test any usage to ensure correct program behavior.
        /// </summary>
        /// <param name="obj">The object whose method is to be invoked.</param>
        /// <param name="methodName">The name of the method to be invoked.</param>
        /// <param name="parameterValues">The values to use when invoking the method.</param>
        /// <returns>The result of the invocation.</returns>
        public static object TryCallMethodWithValues(this object obj, string methodName, params object[] parameterValues)
        {
            return TryCallMethodWithValues(obj, null, methodName, 
                obj is Type ? Flags.StaticAnyVisibility : Flags.InstanceAnyVisibility, parameterValues);
        }

        /// <summary>
        /// Obtains the list of methods for <paramref name="obj"/> using the supplied parameter values
        /// and invokes the best match. This overload requires that the supplied <paramref name="parameterValues"/> 
        /// are all used in the order in which they are supplied. Parameter values can be null.
        /// 
        /// This method is very liberal and attempts to convert values that are not otherwise
        /// considered compatible, such as between strings and enums or numbers, Guids and byte[16], etc.
        /// You should carefully test any usage to ensure correct program behavior.
        /// 
        /// If the default conversion rule doesn't do what you want, you can supply a custom converter.
        /// If it is null, default conversion rule is used.
        /// </summary>
        /// <param name="obj">The object whose method is to be invoked.</param>
        /// <param name="converter">The converter delegate used to perform user-defined conversion.</param>
        /// <param name="methodName">The name of the method to be invoked.</param>
        /// <param name="flags">Binding flags for look up methods.</param>
        /// <param name="parameterValues">The values to use when invoking the method.</param>
        /// <returns>The result of the invocation.</returns>
        public static object TryCallMethodWithValues(this object obj, ParameterConverter converter, string methodName, BindingFlags flags, params object[] parameterValues)
        {
            return TryCallMethodWithValues(obj, converter, methodName,Type.EmptyTypes, flags, parameterValues);
        }


        /// <summary>
        /// Obtains the list of methods for <paramref name="obj"/> using the supplied parameter values
        /// and invokes the best match. This overload requires that the supplied <paramref name="parameterValues"/> 
        /// are all used in the order in which they are supplied. Parameter values can be null.
        /// 
        /// This method is very liberal and attempts to convert values that are not otherwise
        /// considered compatible, such as between strings and enums or numbers, Guids and byte[16], etc.
        /// You should carefully test any usage to ensure correct program behavior.
        /// 
        /// If the default conversion rule doesn't do what you want, you can supply a custom converter.
        /// If it is null, default conversion rule is used.
        /// </summary>
        /// <param name="obj">The object whose method is to be invoked.</param>
        /// <param name="converter">The converter delegate used to perform user-defined conversion.</param>
        /// <param name="methodName">The name of the method to be invoked.</param>
        /// <param name="genericTypes">The type parameter types of the method if it's a generic method.</param>
        /// <param name="flags">Binding flags for look up methods.</param>
        /// <param name="parameterValues">The values to use when invoking the method.</param>
        /// <returns>The result of the invocation.</returns>
        public static object TryCallMethodWithValues(this object obj, ParameterConverter converter, string methodName,
            Type[] genericTypes, BindingFlags flags, 
            params object[] parameterValues)
        {
            var type = obj is Type ? (Type)obj : obj.GetType();
            var methods = type.Methods(genericTypes, null, flags, methodName)
                              .Select(m => m.IsGenericMethodDefinition ? m.MakeGeneric(genericTypes) : m);
            try
            {
                return TryCall(converter, methods, obj, parameterValues);
            }
            catch (MissingMemberException)
            {
                var values = parameterValues ?? new object[0];
                throw new MissingMethodException(string.Format("Unable to locate a matching method {0} on type {1} for parameters: {2}",
                                                                 methodName, type.Name,
                                                                 string.Join(", ", values.Select(v => v == null ? "null" : v.ToString()))));
            }
        }

        /// <summary>
        /// Implementation details:
        /// 
        /// Matching process is done on a shallow copy of parametersValues so that 
        /// the converter could "modify" elements at will.  
        /// 
        /// There will be a problem if the converter modifies a child array and the 
        /// method ends up not being matched (because of another parameter).  
        /// 
        /// The standard Fasterflect converter doesn't modify child array so it's safe.
        /// This is only problematic when a custom converter is provided.
        ///   
        /// TODO How to fix it? a deep clone?
        /// </summary>
        public static object TryCall(ParameterConverter converter, IEnumerable<MethodBase> methodBases, 
            object obj, object[] parameterValues)
        {
            converter = converter ?? new ParameterConverter(StandardConvert);
            if (parameterValues == null)
            {
                parameterValues = new object[0];
            }
            foreach (var mb in GetCandidates(parameterValues, methodBases))
            {
                var convertedArgs = new List<object>();
                var parameters = mb.GetParameters();
                bool isMatch = true;
                for (int paramIndex = 0; paramIndex < parameters.Length; paramIndex++)
                {
                    var parameter = parameters[paramIndex];
                    if (paramIndex == parameters.Length - 1 && IsParams(parameter))
                    {
                        object paramArg;
                        if (parameters.Length - 1 == parameterValues.Length)
                        {
                            paramArg = parameter.ParameterType.CreateInstance(0);
                        }
                        else
                        {
                            paramArg = parameter.ParameterType.CreateInstance(parameterValues.Length - parameters.Length + 1);
                            var elementType = parameter.ParameterType.GetElementType();
                            for (int argIndex = paramIndex; argIndex < parameterValues.Length; argIndex++)
                            {
                                var value = parameterValues[argIndex];
                                if (!converter(elementType, obj, ref value))
                                {
                                    isMatch = false;
                                    goto end_of_loop;
                                }
                                ((Array)paramArg).SetValue( value, argIndex - paramIndex );
                            }
                        }
                        convertedArgs.Add(paramArg);
                    }
                    else
                    {
                        var value = parameterValues[paramIndex];
                        if (!converter(parameter.ParameterType, obj, ref value))
                        {
                            isMatch = false;
                            goto end_of_loop;
                        }
                        convertedArgs.Add(value);
                    }
                }

            end_of_loop:
                if (isMatch)
                {
                    parameterValues = convertedArgs.Count == 0 ? null : convertedArgs.ToArray();
                    return mb is ConstructorInfo
                               ? ((ConstructorInfo) mb).Invoke(parameterValues)
                               : mb.Invoke(obj is Type ? null : obj, parameterValues);
                }
            } // foreach loop
            throw new MissingMemberException();
        }

        private static IEnumerable<MethodBase> GetCandidates(object[] parameterValues, IEnumerable<MethodBase> methodBases)
        {
            return (from methodBase in methodBases
                    let parameters = methodBase.GetParameters()
                    where parameters.Length == parameterValues.Length ||
                          (parameters.Length > 0 && 
                           IsParams(parameters[parameters.Length - 1]) &&
                           parameterValues.Length >= (parameters.Length - 1))
                    orderby parameters.Count()
                    select methodBase).ToList();
        }

        private static bool StandardConvert(Type targetType, object owner, ref object value)
        {
            if( value == null )
                return !typeof(ValueType).IsAssignableFrom( targetType );
            try
            {
                return (value = TypeConverter.Get( targetType, value )) != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool IsParams(ParameterInfo param)
        {
            return param.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0;
        }
    }
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Services\XmlTransformerExtensions.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect
{
    using System;
    using System.Text;

	/// <summary>
	/// This class defines extensions for transforming any object to XML.
	/// </summary>
	internal static class XmlTransformerExtensions
	{
		#region ToXml
		/// <summary>
		/// Generates a string representation of the given <paramref name="obj"/> using the default
		/// <see href="FormatOptions" />. The output will contain one element for every readable
		/// property on <paramref name="obj"/> and process reference properties (other than strings)
		/// recursively. This method does not handle cyclic references - passing in such an object
		/// graph will cause an infinite loop. 
		/// </summary>
		/// <param name="obj">The object to convert to XML.</param>
		/// <returns>A string containing the generated XML data.</returns>
		public static string ToXml( this object obj )
		{
			return ToXml( obj, FormatOptions.Default );
		}

		/// <summary>
		/// Generates a string representation of the given <paramref name="obj"/> using the default
		/// <see href="FormatOptions" />. The output will contain one element for every readable
		/// property on <paramref name="obj"/> and process reference properties (other than strings)
		/// recursively. This method does not handle cyclic references - passing in such an object
		/// graph will cause an infinite loop. 
		/// </summary>
		/// <param name="obj">The object to convert to XML.</param>
		/// <param name="options"></param>
		/// <returns>A string containing the generated XML data.</returns>
		public static string ToXml( this object obj, FormatOptions options )
		{
			bool newLineAfterElement = (options & FormatOptions.NewLineAfterElement) == FormatOptions.NewLineAfterElement;
			string afterElement = newLineAfterElement ? Environment.NewLine : String.Empty;
			bool tabIndent = (options & FormatOptions.UseSpaces) != FormatOptions.UseSpaces;
			string indent = tabIndent ? "\t" : "    ";
			bool addHeader = (options & FormatOptions.AddHeader) == FormatOptions.AddHeader;
			string header = addHeader ? "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine : string.Empty;
			return ToXml( obj, header, afterElement, indent, String.Empty );
		}
		#endregion

		#region ToXml Implementation
		private static string ToXml( object obj, string header, string afterElementDecoration, 
									 string indentDecoration, string currentIndent )
		{
			StringBuilder sb = new StringBuilder();
			Type type = obj.GetType();
			sb.Append( header );
			sb.AppendFormat( "{0}<{1}>{2}", currentIndent, type.Name, afterElementDecoration );

			currentIndent = Indent( indentDecoration, currentIndent );
			// enumerate all instance properties
			foreach( var propertyInfo in type.Properties() )
			{
				// ignore properties we cannot read
				if( propertyInfo.CanRead && propertyInfo.Name != "Item" )
				{
					object propertyValue = propertyInfo.Get( obj );
					Type propertyType = propertyInfo.PropertyType;
					if( (propertyType.IsClass || propertyType.IsInterface) && propertyType != typeof(string) )
					{
					}
					if( (propertyType.IsClass || propertyType.IsInterface) && propertyType != typeof(string) )
					{
						sb.AppendFormat( ToXml( propertyValue, string.Empty, afterElementDecoration, indentDecoration, currentIndent ) );
					}
					else
					{
						sb.AppendFormat( "{0}<{1}>{2}</{1}>{3}",
										 currentIndent,
										 propertyInfo.Name,
										 propertyValue,
										 afterElementDecoration );
					}
				}
			}
			currentIndent = Unindent( indentDecoration, currentIndent );
			sb.AppendFormat( "{0}</{1}>{2}", currentIndent, type.Name, afterElementDecoration );
			return sb.ToString();
		}

		private static string Indent( string indent, string currentIndent )
		{
			return currentIndent + indent;
		}

		private static string Unindent( string indent, string currentIndent )
		{
			return currentIndent.Substring( 0, currentIndent.Length - indent.Length );
		}
		#endregion
	}
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Services\Probing\ConstructorMap.cs
//------------------------------------------------------------------------------
#region License

// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/

#endregion


namespace Fasterflect.Probing
{
    using System;
    using System.Reflection;

	internal class ConstructorMap : MethodMap
	{
		private ConstructorInvoker invoker;

		public ConstructorMap( ConstructorInfo constructor, string[] paramNames, Type[] parameterTypes, 
			                   object[] sampleParamValues, bool mustUseAllParameters )
            : base(constructor, paramNames, parameterTypes, sampleParamValues, mustUseAllParameters)
		{
		}

		#region UpdateMembers Private Helper Method
		private void UpdateMembers(object target, object[] row)
		{
			for( int i = 0; i < row.Length; i++ )
			{
				if( parameterReflectionMask[ i ] )
				{
					MemberInfo member = members[ i ];
					if( member != null )
					{
						object value = parameterTypeConvertMask[ i ] ? TypeConverter.Get( member.Type(), row[ i ] ) : row[ i ];
						member.Set( target, value );
					}
				}
			}
		}
		#endregion

		public override object Invoke( object[] row )
		{
			object[] methodParameters = isPerfectMatch ? row : PrepareParameters(row);
			object result = invoker.Invoke(methodParameters);
			if (!isPerfectMatch && AnySet(parameterReflectionMask))
				UpdateMembers(result, row);
			return result;
		}

		internal override void InitializeInvoker()
		{
			invoker = type.DelegateForCreateInstance( GetParamTypes() );
		}
	}
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Services\Probing\MapFactory.cs
//------------------------------------------------------------------------------
#region License

// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/

#endregion


namespace Fasterflect.Probing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Fasterflect.Caching;

	/// <summary>
	/// Helper class for producing invocation maps that describe how to create an instance of an object
	/// given a set of parameters. Maps are cached to speed up subsequent requests.
	/// </summary>
	internal static class MapFactory
	{
		/// <summary>
		/// This field contains a dictionary mapping from a particular constructor to all known parameter sets,
		/// each with an associated MethodMap responsible for creating instances of the type using the given
		/// constructor and parameter set.
		/// </summary>
		private static readonly Cache<int, MethodMap> mapCache = new Cache<int, MethodMap>();

		#region Map Construction
		public static MethodMap PrepareInvoke( Type type, string[] paramNames, Type[] parameterTypes,
		                                       object[] sampleParamValues )
		{
            SourceInfo sourceInfo = new SourceInfo(type, paramNames, parameterTypes);
			int hash = sourceInfo.GetHashCode();
			MethodMap map = mapCache.Get( hash );
			if( map == null )
			{
                map = DetermineBestConstructorMatch(type, paramNames, parameterTypes, sampleParamValues);
				mapCache.Insert( hash, map );
			}
			return map;
		}
		#endregion

		#region Map Construction Helpers
		internal static MethodMap DetermineBestConstructorMatch( Type type, string[] paramNames, Type[] parameterTypes,
		                                                        object[] sampleParamValues )
		{
			MethodMap map = DetermineBestMatch( type.GetConstructors(), false, paramNames, parameterTypes, sampleParamValues );
			if( map != null )
			{
				return map;
			}
			var sb = new StringBuilder();
			sb.AppendFormat( "No constructor found for type {0} using parameters:{1}", type.Name, Environment.NewLine );
            sb.AppendFormat("{0}{1}", string.Join(", ", Enumerable.Range(0, paramNames.Length).Select(i => string.Format("{0}:{1}", paramNames[i], parameterTypes[i])).ToArray()), Environment.NewLine);
			throw new MissingMethodException( sb.ToString() );
		}
		internal static MethodMap DetermineBestMethodMatch( IEnumerable<MethodBase> methods, bool mustUseAllParameters, string[] paramNames, 
															Type[] parameterTypes, object[] sampleParamValues )
		{
			MethodMap map = DetermineBestMatch( methods, mustUseAllParameters, paramNames, parameterTypes, sampleParamValues );
			if( map != null )
			{
				return map;
			}
			var sb = new StringBuilder();
			sb.AppendFormat( "No method found ({0} candidates examined) matching the parameters:{1}", methods.ToList().Count, Environment.NewLine );
			//sb.AppendFormat( "{0}{1}", Format( parameters, "=", ", " ), Environment.NewLine );
            sb.AppendFormat("{0}{1}", string.Join(", ", Enumerable.Range(0, paramNames.Length).Select(i => string.Format("{0}:{1}", paramNames[i], parameterTypes[i])).ToArray()), Environment.NewLine);
			throw new MissingMethodException( sb.ToString() );
		}

		private static MethodMap DetermineBestMatch( IEnumerable<MethodBase> methods, bool mustUseAllParameters,
													 string[] paramNames, Type[] parameterTypes, object[] sampleParamValues )
		{
			MethodMap bestMap = null;
			foreach( MethodBase method in methods )
			{
                MethodMap map = CreateMap( method, paramNames, parameterTypes, sampleParamValues, mustUseAllParameters );
				if( map != null && map.IsValid )
				{
					bool isBetter = bestMap == null;
					isBetter |= map.IsPerfectMatch;
					isBetter |= bestMap != null &&
					            (map.Cost < bestMap.Cost ||
					             (map.Cost == bestMap.Cost && map.RequiredParameterCount > bestMap.RequiredParameterCount));
					isBetter &= map.IsValid;
					if( isBetter )
					{
						bestMap = map;
					}
				}
			}
			if( bestMap != null )
			{
				bestMap.InitializeInvoker();
				return bestMap;
			}
			return null;
		}

		private static MethodMap CreateMap( MethodBase method, string[] paramNames, Type[] parameterTypes,
		                                    object[] sampleParamValues, bool mustUseAllParameters )
		{
			if( method.IsConstructor )
			{
                return new ConstructorMap( method as ConstructorInfo, paramNames, parameterTypes, sampleParamValues,
				                           mustUseAllParameters );
			}
            return new MethodMap( method, paramNames, parameterTypes, sampleParamValues, mustUseAllParameters );
		}
		#endregion
	}
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Services\Probing\MethodDispatcher.cs
//------------------------------------------------------------------------------
#region License
// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/
#endregion


namespace Fasterflect.Probing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

	/// <summary>
	/// Collects methods into a pool and allows invocation of the best match given a set of parameters.
	/// </summary>
	internal class MethodDispatcher
	{
		//private readonly List<MethodInfo> methodPool = new List<MethodInfo>();
		private readonly List<MethodBase> methodPool = new List<MethodBase>();

		/// <summary>
		/// Add a method to the list of available methods for this method dispatcher.
		/// </summary>
		/// <param name="method">The method to add to the pool of invocation candidates.</param>
		public void AddMethod( MethodInfo method )
		{
			if( method.IsStatic )
			{
				throw new ArgumentException( "Method dispatching currently only supports instance methods.", method.Name );
			}
			if( method.IsAbstract )
			{
				throw new ArgumentException( "Method dispatching does not support abstract methods.", method.Name );
			}
			methodPool.Add( method );
		}

		/// <summary>
		/// Invoke the best available match for the supplied parameters. 
		/// If no method can be called using the supplied parameters, an exception is thrown.
		/// </summary>
		/// <param name="obj">The object on which to invoke a method.</param>
		/// <param name="mustUseAllParameters">Specifies whether all supplied parameters must be used in the
		/// invocation. Unless you know what you are doing you should pass true for this parameter.</param>
		/// <param name="sample">The object whose public properties will be used as parameters.</param>
		/// <returns>The return value of the invocation.</returns>
		public object Invoke( object obj, bool mustUseAllParameters, object sample )
		{
			Type sourceType = sample.GetType();
			var sourceInfo = new SourceInfo( sourceType );
			bool isStatic = obj is Type;
			string[] names = sourceInfo.ParamNames;
			Type[] types = sourceInfo.ParamTypes;
			object[] values = sourceInfo.GetParameterValues( sample );
			if( names.Length != values.Length || names.Length != types.Length )
			{
				throw new ArgumentException( "Mismatching name, type and value arrays (must be of identical length)." );
			}
			MethodMap map = MapFactory.DetermineBestMethodMatch( methodPool, mustUseAllParameters, names, types, values );
			return isStatic ? map.Invoke( values ) : map.Invoke( obj, values );
		}

		/// <summary>
		/// Invoke the best available match for the supplied parameters. 
		/// If no method can be called using the supplied parameters, an exception is thrown.
		/// </summary>
		/// <param name="obj">The object on which to invoke a method.</param>
		/// <param name="mustUseAllParameters">Specifies whether all supplied parameters must be used in the
		/// invocation. Unless you know what you are doing you should pass true for this parameter.</param>
		/// <param name="parameters">A dictionary of parameter name/value pairs.</param>
		/// <returns>The return value of the invocation.</returns>
		public object Invoke( object obj, bool mustUseAllParameters, Dictionary<string, object> parameters )
		{
			bool isStatic = obj is Type;
			string[] names = parameters.Keys.ToArray() ?? new string[0];
			object[] values = parameters.Values.ToArray() ?? new object[0];
			Type[] types = values.ToTypeArray() ?? new Type[0];
			if( names.Length != values.Length || names.Length != types.Length )
			{
				throw new ArgumentException( "Mismatching name, type and value arrays (must be of identical length)." );
			}
			MethodMap map = MapFactory.DetermineBestMethodMatch( methodPool, mustUseAllParameters, names, types, values );
			return isStatic ? map.Invoke( values ) : map.Invoke( obj, values );
		}
	}
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Services\Probing\MethodMap.cs
//------------------------------------------------------------------------------
#region License

// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/

#endregion


namespace Fasterflect.Probing
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;

	/// <summary>
	/// This class wraps a single invokable method call. It contains information on the method to call as well as 
	/// the parameters to use in the method call.
	/// This intermediary class is used by the various other classes to select the best match to call
	/// from a given set of available methods/constructors (and a set of parameter names and types).
	/// </summary>
	internal class MethodMap
	{
		#region Fields
		private readonly bool mustUseAllParameters;
		protected long cost;
		protected bool isPerfectMatch;
		protected bool isValid;
		protected MemberInfo[] members;
		protected MethodBase method;
		protected BitArray methodParameterUsageMask; // marks method parameters for which a source was found
		protected string[] paramNames;
		protected Type[] paramTypes;
		protected BitArray parameterDefaultValueMask; // marks fields where default values will be used
		protected IDictionary<string, object> parameterDefaultValues;
		// protected BitArray parameterInjectionValueMask; // marks fields where injected values will be used
		// protected BitArray parameterNullValueMask; // marks fields where null values will be used
		protected int[] parameterOrderMap;
		protected int[] parameterOrderMapReverse;
		protected BitArray parameterReflectionMask; // marks parameters set using reflection
		protected BitArray parameterTypeConvertMask; // marks columns that may need type conversion
		protected BitArray parameterUnusedMask; // marks unused fields (columns with no target)
		protected long parameterUsageCount; // number of parameters used in constructor call
		protected BitArray parameterUsageMask; // marks parameters used in method call
		protected IList<ParameterInfo> parameters;
		// method call information
		protected int requiredFoundCount;
		protected int requiredParameterCount;
		protected Type type;
		private MethodInvoker invoker;
		#endregion

		#region Constructors and Initialization
		public MethodMap( MethodBase method, string[] paramNames, Type[] paramTypes, object[] sampleParamValues, bool mustUseAllParameters )
		{
			type = method.DeclaringType;
			this.method = method;
			this.paramNames = paramNames;
			this.paramTypes = paramTypes;
			requiredParameterCount = method.Parameters().Count;
			this.mustUseAllParameters = mustUseAllParameters;
			parameters = method.Parameters();
			InitializeBitArrays( Math.Max( parameters.Count, paramNames.Length ) );
			InitializeMethodMap( sampleParamValues );
		}

		private void InitializeBitArrays( int length )
		{
			methodParameterUsageMask = new BitArray( parameters.Count );
			parameterUsageMask = new BitArray( length );
			parameterUnusedMask = new BitArray( length );
			parameterTypeConvertMask = new BitArray( length );
			parameterReflectionMask = new BitArray( length );
			parameterDefaultValueMask = new BitArray( length );
		}

		#region Map Initialization
		private void InitializeMethodMap( object[] sampleParamValues )
		{
			#region Field initialization
			//int normalCount = 0; // number of fields filled with regular parameter values
			int defaultCount = 0; // number of fields filled using default values
			int nullCount = 0; // number of fields filled using null
			int injectionCount = 0; // number of fields filled using external values (dependency injection aka IoC)
			parameterOrderMap = new int[paramNames.Length];
			for( int i = 0; i < paramNames.Length; i++ )
			{
				parameterOrderMap[ i ] = -1;
			}
			parameterUsageCount = 0;
			members = new MemberInfo[paramNames.Length];
			// use a counter to determine whether we have a column for every parameter
			int noColumnForParameter = parameters.Count;
			// keep a reverse index for later when we check for default values
			parameterOrderMapReverse = new int[noColumnForParameter];
			// explicitly mark unused entries as we may have more parameters than columns
			for( int i = 0; i < noColumnForParameter; i++ )
			{
				parameterOrderMapReverse[ i ] = -1;
			}
			bool isPerfectColumnOrder = true;
			#endregion
			
			#region Input parameters loop
			for( int invokeParamIndex = 0; invokeParamIndex < paramNames.Length; invokeParamIndex++ )
			{
				#region Method parameters loop
				string paramName = paramNames[ invokeParamIndex ];
				Type paramType = paramTypes[ invokeParamIndex ];
				bool foundParam = false;
				int methodParameterIndex = 0;
				string errorText = null;
				for( int methodParamIndex = 0; methodParamIndex < parameters.Count; methodParamIndex++ )
				{
					if( methodParameterUsageMask[ methodParamIndex ] ) // ignore input if we already have an appropriate source
					{
						continue;
					}
					methodParameterIndex = methodParamIndex; // preserve loop variable outside loop
					ParameterInfo parameter = parameters[ methodParamIndex ];
					// permit casing differences to allow for matching lower-case parameters to upper-case properties
					if( parameter.HasName( paramName ) )
					{
						bool compatible = parameter.ParameterType.IsAssignableFrom( paramType );
						// avoid checking if implicit conversion is possible
						bool convertible = ! compatible && IsConvertible( paramType, parameter.ParameterType, sampleParamValues[ invokeParamIndex ] );
						if( compatible || convertible )
						{
							foundParam = true;
							methodParameterUsageMask[ methodParamIndex ] = true;
							noColumnForParameter--;
							parameterUsageCount++;
							parameterUsageMask[ invokeParamIndex ] = true;
							parameterOrderMap[ invokeParamIndex ] = methodParamIndex;
							parameterOrderMapReverse[ methodParamIndex ] = invokeParamIndex;
							isPerfectColumnOrder &= invokeParamIndex == methodParamIndex;
							// type conversion required for nullable columns mapping to not-nullable system type
							// or when the supplied value type is different from member/parameter type
							if( convertible )
							{
								parameterTypeConvertMask[ invokeParamIndex ] = true;
								cost += 1;
							}
							break;
						}
						// save a partial exception message in case there is also not a matching member we can set
						errorText = string.Format( "constructor parameter {0} of type {1}", parameter.Name, parameter.ParameterType );
					}
				}
				// method can only be invoked if we have the required number of parameters
				// parameters are checked from left to right (so any required number wont be enough)
				if( foundParam && methodParameterIndex < requiredParameterCount )
				{
					requiredFoundCount++;
				}
				#endregion

				#region No parameter handling (member check)
				if( ! foundParam && method is ConstructorInfo )
				{
					// check if we can use reflection to set some members
					MemberInfo member = type.Property( paramName, Flags.InstanceAnyVisibility | Flags.IgnoreCase );
					// try again using leading underscore if nothing was found
					member = member ?? type.Property( "_" + paramName, Flags.InstanceAnyVisibility | Flags.IgnoreCase );
					// look for fields if we still got no match or property was readonly
					if( member == null || ! member.IsWritable() )
					{
						member = type.Field( paramName, Flags.InstanceAnyVisibility | Flags.IgnoreCase );
						// try again using leading underscore if nothing was found
						member = member ?? type.Field( "_" + paramName, Flags.InstanceAnyVisibility | Flags.IgnoreCase );
					}
					bool exists = member != null; 
					Type memberType = member != null ? member.Type() : null;
					bool compatible = exists && memberType.IsAssignableFrom( paramType );
					// avoid checking if implicit conversion is possible
					bool convertible = exists && ! compatible && IsConvertible( paramType, memberType, sampleParamValues[ invokeParamIndex ] );
					if( method.IsConstructor && (compatible || convertible) )
					{
						members[ invokeParamIndex ] = member;
						// input not included in method call but member field or property is present
						parameterUsageCount++;
						parameterReflectionMask[ invokeParamIndex ] = true;
						cost += 10;
						// flag input parameter for type conversion
						if( convertible )
						{
							parameterTypeConvertMask[ invokeParamIndex ] = true;
							cost += 1;
						}
					}
					else
					{
						// unused column - not in constructor or as member field
						parameterUnusedMask[ invokeParamIndex ] = true;
						if( exists || errorText != null )
						{
							errorText = errorText ?? string.Format( "member {0} of type {1}", member.Name, memberType );
							string message = "Input parameter {0} of type {1} is incompatible with {2} (conversion was not possible).";
							message = string.Format( message, paramName, paramType, errorText );
							throw new ArgumentException( message, paramName );
						}
					}
				}
				#endregion
			}
			#endregion

			#region Default value injection
			// check whether method has unused parameters
			/*
			if( noColumnForParameter > 0 )
			{
				for( int methodParamIndex = 0; methodParamIndex < parameters.Count; methodParamIndex++ )
				{
					int invokeIndex = parameterOrderMapReverse[ methodParamIndex ];
					bool hasValue = invokeIndex != -1;
					if( hasValue )
					{
						hasValue = parameterUsageMask[ invokeIndex ];
					}
					// only try to supply default values for parameters that do not already have a value
					if( ! hasValue )
					{
						ParameterInfo parameter = parameters[ methodParamIndex ];
						bool hasDefaultValue = parameter.HasDefaultValue();
						// default value can be a null value, but not for required parameters
						bool isDefaultAllowed = methodParamIndex >= requiredParameterCount || hasDefaultValue;
						if( isDefaultAllowed )
						{
							// prefer any explicitly defined default parameter value for the parameter
							if( hasDefaultValue )
							{
								SaveDefaultValue( parameter.Name, parameter.DefaultValue );
								parameterDefaultValueMask[ methodParamIndex ] = true;
								defaultCount++;
								noColumnForParameter--;
							}
							else if( HasExternalDefaultValue( parameter ) ) // external values (dependency injection)
							{
								SaveDefaultValue( parameter.Name, GetExternalDefaultValue( parameter ) );
								parameterDefaultValueMask[ methodParamIndex ] = true;
								injectionCount++;
								noColumnForParameter--;
							}
							else // see if we can use null as the default value
							{
								if( parameter.ParameterType != null && parameter.IsNullable() )
								{
									SaveDefaultValue( parameter.Name, null );
									parameterDefaultValueMask[ methodParamIndex ] = true;
									nullCount++;
									noColumnForParameter--;
								}
							}
						}
					}
				}
			}
		 	*/
			#endregion

			#region Cost calculation and map validity checks
			// score 100 if parameter and column count differ
			cost += parameterUsageCount == parameters.Count ? 0 : 100;
			// score 300 if column order does not match parameter order
			cost += isPerfectColumnOrder ? 0 : 300;
			// score 600 if type conversion for any column is required
			cost += AllUnset( parameterTypeConvertMask ) ? 0 : 600;
			// score additinal points if we need to use any kind of default value
			cost += defaultCount * 1000 + injectionCount * 1000 + nullCount * 1000;
			// determine whether we have a perfect match (can use direct constructor invocation)
			isPerfectMatch = isPerfectColumnOrder && parameterUsageCount == parameters.Count;
			isPerfectMatch &= parameterUsageCount == paramNames.Length;
			isPerfectMatch &= AllUnset( parameterUnusedMask ) && AllUnset( parameterTypeConvertMask );
			isPerfectMatch &= cost == 0;
			// isValid tells whether this CM can be used with the given columns
			isValid = requiredFoundCount == requiredParameterCount && parameterUsageCount >= requiredParameterCount;
			isValid &= ! mustUseAllParameters || parameterUsageCount == paramNames.Length;
			isValid &= noColumnForParameter == 0;
			isValid &= AllSet( methodParameterUsageMask );
			// this last specifies that we must use all of the supplied parameters to construct the object
			// isValid &= parameterUnusedMask == 0;
			#endregion
		}

		private bool IsConvertible( Type sourceType, Type targetType, object sampleValue )
		{
			// determine from sample value whether type conversion is needed
			object convertedValue = TypeConverter.Get( targetType, sampleValue );
			return convertedValue != null && sourceType != convertedValue.GetType();
		}

		private void SaveDefaultValue( string parameterName, object parameterValue )
		{
			// perform late initialization of the dictionary for default values
			if( parameterDefaultValues == null )
			{
				parameterDefaultValues = new Dictionary<string, object>();
			}
			parameterDefaultValues[ parameterName ] = parameterValue;
		}
		#endregion
		#endregion

		#region Dependency Injection Helpers
		private bool HasExternalDefaultValue( ParameterInfo parameter )
		{
			// TODO plug in code for DI or DI framework here
			return false;
		}

		private object GetExternalDefaultValue( ParameterInfo parameter )
		{
			return null;
		}
		#endregion

		#region Parameter Preparation
		/// <summary>
		/// Perform parameter reordering, null handling and type conversion in preparation
		/// of executing the method call.
		/// </summary>
		/// <param name="row">The callers row of data.</param>
		/// <returns>The parameter array to use in the actual invocation.</returns>
		protected object[] PrepareParameters( object[] row )
		{
			var methodParams = new object[ parameters.Count ];
			//int firstPotentialDefaultValueIndex = 0;
			for( int i = 0; i < row.Length; i++ )
			{
				// only include columns in constructor
				if( parameterUsageMask[ i ] )
				{
					int index = parameterOrderMap[ i ];
					// check whether we need to type convert the input value
					object value = row[ i ];
					bool convert = parameterTypeConvertMask[ i ];
					convert |= value != null && value.GetType() != paramTypes[ i ];
					if( convert )
					{
						value = TypeConverter.Get( parameters[ index ].ParameterType, row[ i ] );
						if( value == null )
						{
							StringBuilder sb = new StringBuilder();
							sb.AppendFormat( "Input parameter {0} of type {1} could unexpectedly not be converted to type {2}.{3}", 
											 paramNames[ i ], paramTypes[ i ], parameters[ index ].ParameterType, Environment.NewLine );
							sb.AppendFormat( "Conversion was previously possible. Bad input value: {0}", row[ i ] );
							throw new ArgumentException( sb.ToString(), paramNames[ i ] );
						}
					}
					methodParams[ index ] = value;
					// advance counter of sequential fields used to save some time in the loop below
					//if( i == 1 + firstPotentialDefaultValueIndex )
					//{
					//    firstPotentialDefaultValueIndex++;
					//}
				}
			}
			// TODO decide whether to support injecting default values
			//for (int i = firstPotentialDefaultValueIndex; i < methodParams.Length; i++)
			//{
			//    if (parameterDefaultValueMask[i])
			//    {
			//        methodParams[i] = parameterDefaultValues[parameters[i].Name];
			//    }
			//}
			return methodParams;
		}
		#endregion

		#region Method Invocation
		public virtual object Invoke( object[] row )
		{
			throw new NotImplementedException( "This method is implemented in subclasses." );
		}

		public virtual object Invoke( object target, object[] row )
		{
			object[] methodParameters = isPerfectMatch ? row : PrepareParameters( row );
			return invoker.Invoke( target, methodParameters );
		}

		internal Type[] GetParamTypes()
		{
			var paramTypes = new Type[parameters.Count];
			for( int i = 0; i < parameters.Count; i++ )
			{
				ParameterInfo pi = parameters[ i ];
				paramTypes[ i ] = pi.ParameterType;
			}
			return paramTypes;
		}
		#endregion

		#region BitArray Helpers
		/// <summary>
		/// Test whether at least one bit is set in the array. Replaces the old "long != 0" check.
		/// </summary>
		protected bool AnySet( BitArray bits )
		{
			return ! AllUnset( bits );
		}

		/// <summary>
		/// Test whether no bits are set in the array. Replaces the old "long == 0" check.
		/// </summary>
		protected bool AllUnset( BitArray bits )
		{
			foreach( bool bit in bits )
			{
				if( bit )
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Test whether no bits are set in the array. Replaces the old "long == 0" check.
		/// </summary>
		protected bool AllSet( BitArray bits )
		{
			foreach( bool bit in bits )
			{
				if( ! bit )
				{
					return false;
				}
			}
			return true;
		}
		#endregion

		#region Properties
		public IDictionary<string, object> ParameterDefaultValues
		{
			get { return parameterDefaultValues; }
			set { parameterDefaultValues = value; }
		}

		public int ParameterCount
		{
			get { return parameters.Count; }
		}

		public int RequiredParameterCount
		{
			get { return requiredParameterCount; }
		}

		public virtual long Cost
		{
			get { return cost; }
		}

		public bool IsValid
		{
			get { return isValid; }
		}

		public bool IsPerfectMatch
		{
			get { return isPerfectMatch; }
		}
		#endregion

		internal virtual void InitializeInvoker()
		{
			var mi = method as MethodInfo;
			invoker = mi.DelegateForCallMethod();
		}
	}
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Services\Probing\SourceInfo.cs
//------------------------------------------------------------------------------
#region License

// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/

#endregion


namespace Fasterflect.Probing
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

	internal class SourceInfo
	{
		#region Fields
		private bool[] paramKinds;
		private string[] paramNames;
		private Type[] paramTypes;
		private MemberGetter[] paramValueReaders;
		private Type type;
		#endregion

		#region Constructors
		public SourceInfo( Type type )
		{
			this.type = type;
			ExtractParameterInfo( type );
		}

		public SourceInfo( Type type, string[] names, Type[] types )
		{
			this.type = type;
			paramNames = names;
			paramTypes = types;
			paramKinds = new bool[names.Length];
			for (int i = 0; i < paramKinds.Length; i++)
			{
				paramKinds[i] = true;
			}
		}
		#endregion

		#region Properties
		public Type Type
		{
			get { return type; }
		}

		public string[] ParamNames
		{
			get { return paramNames; }
		}

		public Type[] ParamTypes
		{
			get { return paramTypes; }
		}

		public bool[] ParamKinds
		{
			get { return paramKinds; }
		}

		public MemberGetter[] ParamValueReaders
		{
			get
			{
				InitializeParameterValueReaders();
				return paramValueReaders;
			}
		}
		#endregion

		#region Parameter Value Access
		public object[] GetParameterValues(object source)
		{
			InitializeParameterValueReaders();
			var paramValues = new object[paramNames.Length];
			for (int i = 0; i < paramNames.Length; i++)
			{
				paramValues[i] = paramValueReaders[i](source);
			}
			return paramValues;
		}

		internal MemberGetter GetReader(string memberName)
		{
			int index = Array.IndexOf(paramNames, memberName);
			MemberGetter reader = paramValueReaders[index];
			if (reader == null)
			{
				reader = paramKinds[index] ? type.DelegateForGetFieldValue(memberName) : type.DelegateForGetPropertyValue(memberName);
				paramValueReaders[index] = reader;
			}
			return reader;
		}

		private void InitializeParameterValueReaders()
		{
			if (paramValueReaders == null)
			{
				paramValueReaders = new MemberGetter[paramNames.Length];
				for (int i = 0; i < paramNames.Length; i++)
				{
					string name = paramNames[i];
					paramValueReaders[i] = paramKinds[i] ? type.DelegateForGetFieldValue(name) : type.DelegateForGetPropertyValue(name);
				}
			}
		}
		#endregion

		#region Equals + GetHashCode
		public override bool Equals( object obj )
		{
			var other = obj as SourceInfo;
			if (other == null) return false;
			if (other == this) return true;

			if( type != other.Type || paramNames.Length != other.ParamNames.Length )
				return false;
			for( int i = 0; i < paramNames.Length; i++ )
			{
				if( paramNames[ i ] != other.ParamNames[ i ] || paramTypes[ i ] != other.ParamTypes[ i ] )
					return false;
			}
			return true;
		}
		public override int GetHashCode()
		{
			int hash = type.GetHashCode();
			for( int i = 0; i < paramNames.Length; i++ )
			    hash += (i + 31) * paramNames[ i ].GetHashCode() ^ paramTypes[ i ].GetHashCode();
			return hash;
		}
		#endregion

		#region Anonymous Type Helper (ExtractParameterInfo)
		internal void ExtractParameterInfo( Type type )
		{
            IList<MemberInfo> members = type.Members(MemberTypes.Field | MemberTypes.Property, Flags.InstanceAnyVisibility);
			var names = new List<string>(members.Count);
			var types = new List<Type>(members.Count);
			var kinds = new List<bool>(members.Count);
			for (int i = 0; i < members.Count; i++)
			{
				MemberInfo mi = members[i];
				bool include = mi is FieldInfo && mi.Name[0] != '<'; // exclude auto-generated backing fields
				include |= mi is PropertyInfo && (mi as PropertyInfo).CanRead; // exclude write-only properties
				if (include)
				{
					names.Add(mi.Name);
					bool isField = mi is FieldInfo;
					kinds.Add(isField);
					types.Add(isField ? (mi as FieldInfo).FieldType : (mi as PropertyInfo).PropertyType);
				}
			}
			paramNames = names.ToArray();
			paramTypes = types.ToArray();
			paramKinds = kinds.ToArray();
		}
		#endregion
	}
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Extensions\Services\Probing\TypeConverter.cs
//------------------------------------------------------------------------------
#region License

// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/

#endregion


namespace Fasterflect.Probing
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;

	/// <summary>
	/// Helper class for converting values between various types.
	/// </summary>
	internal static class TypeConverter
	{
		#region GetValue methods
		/// <summary>
		/// Convert the supplied XmlNode into the specified target type. Only the InnerXml portion
		/// of the XmlNode is used in the conversion, unless the target type is itself an XmlNode.
		/// </summary>
		/// <param name="targetType">The type into which to convert</param>
		/// <param name="node">The source value used in the conversion operation</param>
		/// <returns>The converted value</returns>
		public static object Get( Type targetType, XmlNode node )
		{
			if( targetType == typeof(XmlNode) )
			{
				return node;
			}
			return Get( targetType, node.InnerXml );
		}
		/// <summary>
		/// Convert the supplied XAttribute into the specified target type. Only the Value portion
		/// of the XAttribute is used in the conversion, unless the target type is itself an XAttribute.
		/// </summary>
		/// <param name="targetType">The type into which to convert</param>
		/// <param name="attribute">The source value used in the conversion operation</param>
		/// <returns>The converted value</returns>
		public static object Get( Type targetType, XAttribute attribute )
		{
			if( targetType == typeof(XAttribute) )
			{
				return attribute;
			}
			return Get( targetType, attribute.Value );
		}
		/// <summary>
		/// Convert the supplied XElement into the specified target type. Only the Value portion
		/// of the XElement is used in the conversion, unless the target type is itself an XElement.
		/// </summary>
		/// <param name="targetType">The type into which to convert</param>
		/// <param name="element">The source value used in the conversion operation</param>
		/// <returns>The converted value</returns>
		public static object Get( Type targetType, XElement element )
		{
			if( targetType == typeof(XElement) )
			{
				return element;
			}
			return Get( targetType, element.Value );
		}

		/// <summary>
		/// Convert the supplied string into the specified target type. 
		/// </summary>
		/// <param name="targetType">The type into which to convert</param>
		/// <param name="value">The source value used in the conversion operation</param>
		/// <returns>The converted value</returns>
		public static object Get( Type targetType, string value )
		{
			Type sourceType = typeof(string);
			if( sourceType == targetType )
			{
				return value;
			}
			try
			{
				if( targetType.IsEnum )
				{
					return ConvertEnums( targetType, sourceType, value );
				}
				if( targetType == typeof(Guid) )
				{
					return ConvertGuids( targetType, sourceType, value );
				}
				if( targetType == typeof(Type) )
				{
					return ConvertTypes( targetType, sourceType, value );
				}
				return !string.IsNullOrEmpty( value ) ? Convert.ChangeType( value, targetType, CultureInfo.InvariantCulture ) : null;
			}
			catch( FormatException )
			{
				return null; // no conversion was possible
			}
		}

	    /// <summary>
		/// Convert the supplied object into the specified target type. 
		/// </summary>
		/// <param name="targetType">The type into which to convert</param>
		/// <param name="value">The source value used in the conversion operation</param>
		/// <returns>The converted value</returns>
		public static object Get( Type targetType, object value )
		{
			if( value == null )
			{
				return null;
			}
			Type sourceType = value.GetType();
			if( sourceType == targetType )
			{
				return value;
			}
			if( sourceType == typeof(string) )
			{
				return Get( targetType, value as string );
			}
			if( sourceType == typeof(XmlNode) )
			{
				return Get( targetType, value as XmlNode );
			}
			if( sourceType == typeof(XAttribute) )
			{
				return Get( targetType, value as XAttribute );
			}
			if( sourceType == typeof(XElement) )
			{
				return Get( targetType, value as XElement );
			}
			if( targetType.IsEnum || sourceType.IsEnum )
			{
				return ConvertEnums( targetType, sourceType, value );
			}
			if( targetType == typeof(Guid) || sourceType == typeof(Guid) )
			{
				return ConvertGuids( targetType, sourceType, value );
			}
			if( targetType == typeof(Type) || sourceType == typeof(Type) )
			{
				return ConvertTypes( targetType, sourceType, value );
			}
			return value is IConvertible ? Convert.ChangeType( value, targetType ) : null;
		}
		#endregion

		#region Type conversions
		/// <summary>
		/// A method that will convert between types and their textual names.
		/// </summary>
		/// <param name="targetType">The target type</param>
		/// <param name="sourceType">The type of the provided value.</param>
		/// <param name="value">The value representing the type.</param>
		/// <returns></returns>
		public static object ConvertTypes( Type targetType, Type sourceType, object value )
		{
			if( value == null )
			{
				return null;
			}
			if( targetType == typeof(Type) )
			{
				return Type.GetType( Convert.ToString( value ) );
			}
			if( sourceType == typeof(Type) && targetType == typeof(string) )
			{
				return sourceType.FullName;
			}
			return null;
		}
		#endregion

		#region Enum conversions
		/// <summary>
		/// Helper method for converting enums from/to different types.
		/// </summary>
		private static object ConvertEnums( Type targetType, Type sourceType, object value )
		{
			if( targetType.IsEnum )
			{
				if( sourceType == typeof(string) )
				{
					// assume the string represents the name of the enumeration element
					string source = (string) value;
					// first try to clean out unnecessary information (like assembly name and FQTN)
					source = source.Split( ',' )[ 0 ];
					int pos = source.LastIndexOf( '.' );
					if( pos > 0 )
						source = source.Substring( pos + 1 ).Trim();
					if( Enum.IsDefined( targetType, source ) )
					{
						return Enum.Parse( targetType, source );
					}
				}
				// convert the source object to the appropriate type of value
				value = Convert.ChangeType( value, Enum.GetUnderlyingType( targetType ) );
				return Enum.ToObject( targetType, value );
			}
			return Convert.ChangeType( value, targetType );
		}
		#endregion

		#region GUID conversions
		/// <summary>
		/// Convert the binary string (16 bytes) into a Guid.
		/// </summary>
		public static Guid StringToGuid( string guid )
		{
			char[] charBuffer = guid.ToCharArray();
			byte[] byteBuffer = new byte[16];
			int nCurrByte = 0;
			foreach( char currChar in charBuffer )
			{
				byteBuffer[ nCurrByte++ ] = (byte) currChar;
			}
			return new Guid( byteBuffer );
		}

		/// <summary>
		/// Convert the Guid into a binary string.
		/// </summary>
		public static string GuidToBinaryString( Guid guid )
		{
			StringBuilder sb = new StringBuilder( 16 );
			foreach( byte currByte in guid.ToByteArray() )
			{
				sb.Append( (char) currByte );
			}
			return sb.ToString();
		}

		/// <summary>
		/// A method that will convert guids from and to different types
		/// </summary>
		private static object ConvertGuids( Type targetType, Type sourceType, object sourceObj )
		{
			if( targetType == typeof(Guid) )
			{
				if( sourceType == typeof(string) )
				{
					string value = sourceObj as string;
					if( value != null && value.Length == 16 )
					{
						return StringToGuid( value );
					}
					return value != null ? new Guid( value ) : Guid.Empty;
				}
				if( sourceType == typeof(byte[]) )
				{
					return new Guid( (byte[]) sourceObj );
				}
			}
			else if( sourceType == typeof(Guid) )
			{
				Guid g = (Guid) sourceObj;
				if( targetType == typeof(string) )
				{
					return GuidToBinaryString( g );
				}
				if( targetType == typeof(byte[]) )
				{
					return g.ToByteArray();
				}
			}
			return null;
			// Check.FailPostcondition( typeof(TypeConverter), "Cannot convert type {0} to type {1}.", sourceType, targetType );
		}
		#endregion
	}
}


//------------------------------------------------------------------------------
// File: C:\Junk\Fasterflect\Properties\AssemblyInfo.cs
//------------------------------------------------------------------------------

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.


// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.


// The following GUID is for the ID of the typelib if this project is exposed to COM


// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]

