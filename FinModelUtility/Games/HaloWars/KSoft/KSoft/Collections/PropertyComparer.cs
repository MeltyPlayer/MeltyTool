using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Collections.Generic
{
	// Class based on ideas presented in this article: http://www.codeproject.com/KB/miscctrl/GenericComparer.aspx

	public sealed class PropertyComparer<T>
		: Comparer<T>
	{
		readonly System.Reflection.PropertyInfo mProperty;
		readonly SortDirection mDirection;

		/// <summary>Validate the property used for this comparison is valid</summary>
		/// <param name="type"></param>
		/// <param name="checkPropertyOwner">Should we validate that the property is a member of <paramref name="type"/>?</param>
		/// <exception cref="MemberAccessException" />
		/// <exception cref="MissingMemberException" />
		void ValidateProperty(Type type, bool checkPropertyOwner = false)
		{
			if (!this.mProperty.CanRead)
			{
				throw new MemberAccessException(string.Format(Util.InvariantCultureInfo,
					"{0}'s property '{1}' can't be read!",
					type.Name,
					this.mProperty.Name));
			}

			if (checkPropertyOwner && this.mProperty.DeclaringType != type)
			{
				throw new MissingMemberException(string.Format(Util.InvariantCultureInfo,
					"Property '{1}' is not a member of {0}!",
					type.Name,
					this.mProperty.Name));
			}
		}

		#region Ctor
		/// <summary>Build a new comparer based on existing property info</summary>
		/// <param name="property">Existing property info. Cannot be null.</param>
		/// <param name="direction">Direction of comparison results</param>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="MemberAccessException" />
		/// <exception cref="MissingMemberException" />
		public PropertyComparer(System.Reflection.PropertyInfo property, SortDirection direction = SortDirection.Ascending)
		{
			Contract.Requires<ArgumentNullException>(property != null);

			var type = typeof(T);

			this.mProperty = property;
			this.mDirection = direction;

			this.ValidateProperty(type, true);
		}
		/// <summary>Build a new comparer</summary>
		/// <param name="propertyName">Property of <typeparamref name="T"/> to compare, or null</param>
		/// <param name="direction">Direction of comparison results</param>
		/// <exception cref="MemberAccessException" />
		/// <exception cref="MissingMemberException" />
		public PropertyComparer(string propertyName = null, SortDirection direction = SortDirection.Ascending)
		{
			var type = typeof(T);

			if (string.IsNullOrEmpty(propertyName))
			{
				var properties = type.GetProperties();
				if (properties.Length > 0)
				{
					this.mProperty = properties[0];
				}
				else
				{
					throw new MissingMemberException(string.Format(Util.InvariantCultureInfo,
						"{0} does not contain any properties",
						type.Name));
				}
			}
			else
			{
				var prop = type.GetProperty(propertyName);
				if (prop != null)
				{
					this.mProperty = prop;
				}
				else
				{
					throw new MissingMemberException(string.Format(Util.InvariantCultureInfo,
						"{0} does not contain a property named '{1}'",
						type.Name, propertyName));
				}
			}

			this.mDirection = direction;

			this.ValidateProperty(type);
		}
		/// <summary>Build a default comparer with <see cref="SortDirection.Ascending">Ascending</see> results</summary>
		public PropertyComparer()
			: this((string)null, SortDirection.Ascending)
		{
		}
		#endregion

		public override int Compare(T x, T y)
		{
			// #REVIEW: would be better off having a variant impl that generates a LINQ expression for the compare.
			// Would require a TProp (property's type) generic param, but we'd also be avoiding any boxing
			// operations so long as we constrain TProp : IComparable<TProp>

			// #REVIEW: I think it's safe to cast to IComparable<T>
			// unless you're still using .NET 1 assemblies...why would you do such a thing?
			var obj1 = this.mProperty.GetValue(x, null) as IComparable;
			var obj2 = this.mProperty.GetValue(y, null) as IComparable;

			int result = obj1.CompareTo(obj2);

			if (this.mDirection != SortDirection.Ascending)
				result *= -1;

			return result;
		}

		public static PropertyComparer<T> SortBy(string propertyName,
			SortDirection direction = SortDirection.Ascending)
		{
			return new PropertyComparer<T>(propertyName, direction);
		}
	};
}
