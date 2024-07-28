using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Values
{
	/// <summary>Base interface for Group Tag identifier definitions</summary>
	[Contracts.ContractClass(typeof(GroupTagDataContract))]
//	[System.ComponentModel.TypeConverter(typeof(GroupTagDataConverter))]
	[SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes")]
	public abstract class GroupTagData
		: IO.IEndianStreamable
		, IComparer<GroupTagData>, IComparable<GroupTagData>
		, IEquatable<GroupTagData>, IEqualityComparer<GroupTagData>
		, System.Collections.IComparer, IComparable
	{
		#region Name
		public const string kNullGroupName = TypeExtensions.kNoneDisplayString;
		public const int kGroupNamePadLength = 64;

		readonly string mName;
		/// <summary>Full name of this group</summary>
		public string Name { get => this.mName; }

		/// <summary>Formats <see cref="Name"/> to a properly (left) aligned string (using blank-white-space)</summary>
		/// <returns><see cref="string.PadLeft"/> on <see cref="Name"/></returns>
		/// <remarks>Pad width is determined by <see cref="kGroupNamePadLength"/></remarks>
		public string NameToLeftPaddedString()	=> this.mName.PadLeft(kGroupNamePadLength);
		/// <summary> Formats <see cref="Name"/> to a properly (right) aligned string (using blank-white-space)</summary>
		/// <returns><see cref="string.PadRight"/> on <see cref="Name"/></returns>
		/// <remarks>Pad width is determined by <see cref="kGroupNamePadLength"/></remarks>
		public string NameToRightPaddedString()	=> this.mName.PadRight(kGroupNamePadLength);
		#endregion

		#region Tag
		readonly string mTagAsString;
		readonly char[] mTag;
		/// <summary>The character code of this group</summary>
		[System.ComponentModel.Browsable(false)]
		[SuppressMessage("Microsoft.Design", "CA1819:PropertiesShouldNotReturnArrays")]
		public char[] Tag { get => this.mTag; }
		/// <summary>Get the character code of this group as a string</summary>
		public string TagString { get => this.mTagAsString; }
		#endregion

		/// <summary>Extra data that can be tagged to this Group Tag</summary>
		[System.ComponentModel.Browsable(false)]
		public object UserData { get; private set; }

		/// <summary>Guid for this group tag</summary>
		public KGuid Uuid	{ get; private set; } = KGuid.Empty;

#if false // ObjectInvariant moot, as all non-user properties are readonly
		[Contracts.ContractInvariantMethod]
		void ObjectInvariant()
		{
			Contract.Invariant(!string.IsNullOrEmpty(mName));
			Contract.Invariant(mTag != null);
			Contract.Invariant(!string.IsNullOrEmpty(mTagAsString));
		}
#endif

		#region Ctor
		/// <summary>Only for Null Constructors</summary>
		protected GroupTagData(int expectedLength)
		{
			Contract.Assume(expectedLength > 0);

			this.mName = kNullGroupName;

			this.mTag = new char[expectedLength];
			for (int x = 0; x < this.mTag.Length; x++)
				this.mTag[x] = (char)0xFF;

			this.mTagAsString = new string(this.mTag);
		}
		/// <summary>Initialize a group tag from a character code and name</summary>
		/// <param name="groupTag">Character code string</param>
		/// <param name="name">Name of this group tag</param>
		/// <param name="expectedLength">Expected length of the <paramref name="groupTag"/></param>
		protected GroupTagData(string groupTag, string name, int expectedLength)
		{
#if false
			Contract.Requires(!string.IsNullOrEmpty(groupTag));
			Contract.Requires(!string.IsNullOrEmpty(name));
			Contract.Requires<ArgumentOutOfRangeException>(groupTag.Length == expectedLength);
#endif
			Contract.Requires<ArgumentException>(name != kNullGroupName, "Name reserved for null group tags");

			Util.MarkUnusedVariable(ref expectedLength); // #REVIEW: why did I leave the Requires using this commented out?

			this.mName = name;
			this.mTagAsString = groupTag;
			this.mTag = groupTag.ToCharArray();
		}
		/// <summary>Initialize a group tag from a character code, name and <see cref="Guid"/></summary>
		/// <param name="groupTag">Character code string</param>
		/// <param name="name">Name of this group tag</param>
		/// <param name="uuid">Guid for this group tag</param>
		/// <param name="expectedLength">Expected length of the <paramref name="groupTag"/></param>
		protected GroupTagData(string groupTag, string name, KGuid uuid, int expectedLength) : this(groupTag, name, expectedLength)
		{
#if false
			Contract.Requires(!string.IsNullOrEmpty(groupTag));
			Contract.Requires(!string.IsNullOrEmpty(name));
			Contract.Requires<ArgumentOutOfRangeException>(groupTag.Length == expectedLength);
			Contract.Requires(uuid != KGuid.Empty);
#endif

			this.Uuid = uuid;
		}
		/// <summary>Specialized ctor for <see cref="GroupTagData64"/> built from two <see cref="GroupTagData32"/></summary>
		/// <param name="maj">First four-character code</param>
		/// <param name="min">Second four-character code</param>
		/// <param name="name">Name of this group tag</param>
		/// <remarks>Constructs a group tag in the form of '<paramref name="maj"/>' + '<paramref name="min"/>'</remarks>
		protected GroupTagData(GroupTagData32 maj, GroupTagData32 min, string name)
		{
#if false
			Contract.Requires(maj != null && maj != GroupTagData32.Null);
			Contract.Requires(min != null && min != GroupTagData32.Null);
			Contract.Requires(!string.IsNullOrEmpty(name));
#endif
			Contract.Requires<ArgumentException>(name != kNullGroupName, "Name reserved for null group tags");

			this.mName = name;
			this.mTagAsString = string.Format(Util.InvariantCultureInfo,
			                                  "{0}{1}", maj.mTagAsString, min.mTagAsString);
			this.mTag = this.mTagAsString.ToCharArray();
		}
		/// <summary>Specialized ctor for <see cref="GroupTagData64"/> built from two <see cref="GroupTagData32"/></summary>
		/// <param name="maj">First four-character code</param>
		/// <param name="min">Second four-character code</param>
		/// <param name="name">Name of this group tag</param>
		/// <param name="uuid">Guid for this group tag</param>
		/// <remarks>Constructs a group tag in the form of '<paramref name="maj"/>' + '<paramref name="min"/>'</remarks>
		protected GroupTagData(GroupTagData32 maj, GroupTagData32 min, string name, KGuid uuid) : this(maj, min, name)
		{
#if false
			Contract.Requires(maj != null && maj != GroupTagData32.Null);
			Contract.Requires(min != null && min != GroupTagData32.Null);
			Contract.Requires(!string.IsNullOrEmpty(name));
			Contract.Requires(uuid != KGuid.Empty);
#endif

			this.Uuid = uuid;
		}
		#endregion


		#region Overrides
		public override string ToString() => string.Format(Util.InvariantCultureInfo,
			"['{0," + (this is GroupTagData32 ? "4" : "8") + "}'  {1}]",
			this.TagString,
			this.Name);

		public override int GetHashCode() => this.Name.GetHashCode();

		public override bool Equals(object obj)
		{
			if (obj is GroupTagData g)
				return this.Equals(g);

			return false;
		}
		#endregion

		#region Operators
		public static bool operator ==(GroupTagData a, GroupTagData b) => a.Equals(b);
		public static bool operator !=(GroupTagData a, GroupTagData b) => !(a == b);

		/// <summary>Returns the name of this group tag</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static explicit operator string(GroupTagData value)
		{
			Contract.Requires(value != null);

			return value.mName;
		}

		/// <summary>Returns the group tag in char[] form</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[SuppressMessage("Microsoft.Design", "CA2225:OperatorOverloadsHaveNamedAlternates")]
		public static explicit operator char[](GroupTagData value)
		{
			Contract.Requires(value != null);

			return value.mTag;
		}
		#endregion

		/// <summary>
		/// Takes another character code and performs a check on it against this
		/// object's tag to see if they are completely equal
		/// </summary>
		/// <param name="other"></param>
		/// <returns>True if equal to this</returns>
		public abstract bool Test(char[] other);
		/// <summary>Is this Group Tag equal to the "null" equivalent value?</summary>
		public abstract bool IsNull { get; }

		#region Comparing members (ID based)
		/// <summary>Compare this group tag's ID to another</summary>
		/// <param name="other"></param>
		/// <returns>this ID - <paramref name="other"/>'s ID</returns>
		public int CompareId(GroupTagData other)
		{
			Contract.Requires(other != null);

			return this.GetHashCode() - other.GetHashCode();
		}
		#endregion


		#region Compare Members (name based)
		/// <summary>Does a comparison based on the tag group's names</summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		/// <seealso cref="String.Compare(String, String, bool)"/>
		public int Compare(GroupTagData x, GroupTagData y)
		{
			Contract.Assume(x != null);
			Contract.Assume(y != null);

			return string.Compare(x.mName, y.mName, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>Does a comparison based on the group tag's names</summary>
		/// <param name="other"></param>
		/// <returns></returns>
		/// <seealso cref="String.Compare(String, String, bool)"/>
		public int CompareTo(GroupTagData other)
		{
			Contract.Assume(other != null);

			return string.Compare(this.mName, other.mName, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>Does a comparison based on the tag group's names</summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		/// <seealso cref="String.Compare(String, String, bool)"/>
		public int Compare(object x, object y)
		{
			Contract.Assume(x != null);
			Contract.Assume(y != null);

			return this.Compare((GroupTagData)x, (GroupTagData)y);
		}
		/// <summary>Does a comparison based on the tag group's names</summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		/// <seealso cref="String.Compare(String, String, bool)"/>
		public int CompareTo(object obj)
		{
			Contract.Assume(obj != null);

			return this.CompareTo((GroupTagData)obj);
		}
		#endregion

		#region IEquatable & IEqualityComparer Members
		/// <summary>Compares this to another <see cref="GroupTagData"/> object testing their "ID" fields for equality</summary>
		/// <param name="other"></param>
		/// <returns>true if both this object and <paramref name="obj"/> are equal</returns>
		public abstract bool Equals(GroupTagData other);
		/// <summary>Compares two <see cref="GroupTagData"/> objects testing their "ID" fields for equality</summary>
		/// <param name="x">left-hand value for comparison expression</param>
		/// <param name="y">right-hand value for comparison expression</param>
		/// <returns>true if both <paramref name="x"/> and <paramref name="y"/> are equal</returns>
		public bool Equals(GroupTagData x, GroupTagData y)
		{
			Contract.Assume(x != null);
			Contract.Assume(y != null);

			return x.Equals(y);
		}
		/// <summary>Returns the hash code for this instance</summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public int GetHashCode(GroupTagData obj)
		{
			Contract.Assume(obj != null);

			return obj.GetHashCode();
		}
		#endregion

		#region IEndianStreamable Members
		public abstract void Read(IO.EndianReader s);

		public abstract void Write(IO.EndianWriter s);
		#endregion
	};
	[Contracts.ContractClassFor(typeof(GroupTagData))]
	abstract class GroupTagDataContract : GroupTagData
	{
		GroupTagDataContract() : base(4) {}

		public override bool Test(char[] other)
		{
			Contract.Requires(other != null);
			Contract.Requires<ArgumentOutOfRangeException>(other.Length == this.Tag.Length);

			throw new NotImplementedException();
		}

		public override bool Equals(GroupTagData other)
		{
			Contract.Requires(other != null);

			throw new NotImplementedException();
		}
	};

	/// <summary>Base attribute for declaring a <see cref="GroupTagData"/> on a <b>class</b> or <b>struct</b></summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
	public abstract class GroupTagDataAttribute : Attribute
	{
		/// <summary>Get the <see cref="GroupTagData"/> this attribute defines</summary>
		public abstract GroupTagData GroupTagData { get; }
	};
}
