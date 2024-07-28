using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Values
{
	/// <summary>Attribute applied to classes which house a static <see cref="GroupTagCollection"/> property</summary>
	/// <remarks>
	/// Allows for ease-of-use in other attributes where we'd need to index a <see cref="GroupTagCollection"/>
	/// collection for a specific <see cref="GroupTagData"/> member
	/// </remarks>
	/// <seealso cref="Shell.PlatformGroups"/>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
	public abstract class GroupTagContainerAttribute : Attribute
	{
		/// <summary>Default property name used when looking up the "main" group collection</summary>
		public const string kDefaultName = "Groups";

		#region Ctor
		Type mHost;
		protected GroupTagContainerAttribute(Type container)
		{
			Contract.Requires(container != null);

			this.mHost = container;

			this.FindStaticGroupsProperty();
			this.FindAllStaticGroupFields();
		}

		protected GroupTagContainerAttribute(Type container, string collectionName)
		{
			Contract.Requires(container != null);
			Contract.Requires(!string.IsNullOrEmpty(collectionName));

			this.mHost = container;

			this.FindStaticGroupsProperty(collectionName);
			this.FindAllStaticGroupFields();
		}
		#endregion

		#region Find collections
		protected GroupTagCollection TagCollection { get; private set; }
		void FindStaticGroupsProperty(string collectionName = null)
		{
			if (string.IsNullOrEmpty(collectionName)) collectionName = kDefaultName;

			var pi = this.mHost.GetProperty(collectionName, BindingFlags.Public | BindingFlags.Static);
			if (pi == null) throw new ArgumentException(
				string.Format(Util.InvariantCultureInfo,
					"[{0}] doesn't have a static collection property named '{1}'",
					this.mHost.FullName, collectionName),
					nameof(collectionName));

			this.TagCollection = pi.GetValue(null, null) as GroupTagCollection;
		}

		private IEnumerable<KeyValuePair<string, GroupTagCollection>> mAllCollections;
		void FindAllStaticGroupFields()
		{
			var pis = this.mHost.GetProperties(BindingFlags.Public | BindingFlags.Static);

			var tagc = from p in pis
					   where p.PropertyType.IsSubclassOf(typeof(GroupTagCollection))
					   //select p.GetValue(null, null) as GroupTagCollection;
					   select new KeyValuePair<string, GroupTagCollection>(p.Name, p.GetValue(null, null) as GroupTagCollection);

			this.mAllCollections = tagc;
		}
		#endregion

		/// <summary>Get the "main" <see cref="GroupTagCollection"/> property value from a group tag container</summary>
		/// <param name="container">Type which acts as a <see cref="GroupTagCollection"/> container</param>
		/// <returns></returns>
		public static GroupTagCollection GetCollection(Type container)
		{
			Contract.Requires(container != null);
			Contract.Ensures(Contract.Result<GroupTagCollection>() != null);

			var attr = container.GetCustomAttributes(typeof(GroupTagContainerAttribute), false);

			if (attr.Length != 1)
				throw new ArgumentException(string.Format(Util.InvariantCultureInfo, "[{0}] doesn't have a ", container.FullName), nameof(container));

			return (attr[0] as GroupTagContainerAttribute).TagCollection;
		}
		/// <summary>Get all <see cref="GroupTagCollection"/> property values from a group tag container</summary>
		/// <param name="container">Type which acts as a <see cref="GroupTagCollection"/> container</param>
		/// <returns>
		/// Enumeration of property names (as they appear in <paramref name="container"/>'s definition)
		/// and their respected values.
		/// </returns>
		/// <remarks>The "main" group is still included in the resulting enumeration</remarks>
		public static IEnumerable<KeyValuePair<string, GroupTagCollection>> GetAllCollections(Type container)
		{
			Contract.Requires(container != null);
			Contract.Ensures(Contract.Result<IEnumerable<KeyValuePair<string, GroupTagCollection>>>() != null);

			var attr = container.GetCustomAttributes(typeof(GroupTagContainerAttribute), false);

			if (attr.Length != 1)
				throw new ArgumentException(string.Format(Util.InvariantCultureInfo, "[{0}] doesn't have a ", container.FullName), nameof(container));

			return (attr[0] as GroupTagContainerAttribute).mAllCollections;
		}
	};
}
