using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Values
{
	/// <summary>Base interface for Group Tag collections</summary>
	[Contracts.ContractClass(typeof(GroupTagCollectionContract))]
	public abstract class GroupTagCollection
		: IReadOnlyList<GroupTagData>
		, IO.IEndianStreamable
	{
		/// <summary>Compare two <see cref="GroupTagData"/> objects by their <see cref="GroupTagData.Name"/> properties</summary>
		class ByNameComparer
			: System.Collections.IComparer
			, IComparer<GroupTagData>
		{
			public int Compare(object x, object y)
			{
				Contract.Assume(x != null);
				Contract.Assume(y != null);

				return this.Compare((GroupTagData)x, (GroupTagData)y);
			}
			public int Compare(GroupTagData x, GroupTagData y)
			{
				Contract.Assume(x != null);
				Contract.Assume(y != null);

				return string.CompareOrdinal(x.Name, y.Name);
			}
		};

		[SuppressMessage("Microsoft.Design", "CA1819:PropertiesShouldNotReturnArrays")]
		protected abstract GroupTagData[] BaseGroupTags { get; }

		/// <summary>Get the group tag which represents null</summary>
		public abstract GroupTagData NullGroupTag { get; }

		/// <summary>Guid for this group tag collection</summary>
		public KGuid Uuid { get; private set; } = KGuid.Empty;

		/// <summary>Does this instance represent an empty collection of the concrete GroupTagData type?</summary>
		/// <remarks>If the Uuid is not zero, but there are no GroupTags, this will return false</remarks>
		public bool IsEmpty { get => this.BaseGroupTags.Length == 0 && this.Uuid == KGuid.Empty; }

		protected GroupTagCollection(GroupTagData[] groups)
		{
			Contract.Requires(Array.TrueForAll(groups, Predicates.IsNotNull));
		}
		protected GroupTagCollection(GroupTagData[] groups, KGuid uuid) : this(groups)
		{
			this.Uuid = uuid;
		}

		#region Indexers
		/// <summary>Get the full name of a group tag based on its character code</summary>
		/// <remarks>If <paramref name="tag"/> is not found, "unknown" is returned</remarks>
		[Contracts.Pure]
		[SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
		public string this[char[] tag] { get {
			Contract.Requires(tag != null);
			Contract.Requires(tag.Length == this.NullGroupTag.Tag.Length,
				"Tag lengths mismatch");
			Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

			foreach (GroupTagData t in this.BaseGroupTags)
			{
				if (t.Test(tag))
					return t.Name;
			}

			return this.NullGroupTag.Name;
		} }
		#endregion

		#region Searching
		/// <summary>Finds the index of a <see cref="GroupTagData"/></summary>
		/// <param name="groupTag">The <see cref="GroupTagData"/>'s 'tag' to search for</param>
		/// <returns>Index of <paramref name="groupTag"/> or <b>-1</b> if not found</returns>
		[Contracts.Pure]
		public int FindGroupIndexByTag(char[] groupTag)
		{
			Contract.Requires(groupTag != null);
			Contract.Requires(groupTag.Length == this.NullGroupTag.Tag.Length,
				"Tag lengths mismatch");

			return this.BaseGroupTags.FindIndex(gt => gt.Test(groupTag));
		}
		/// <summary>Finds the index of a <see cref="GroupTagData"/></summary>
		/// <param name="tagString">The <see cref="GroupTagData"/>'s 'tag' to search for</param>
		/// <returns>Index of <paramref name="tagString"/> or <b>-1</b> if not found</returns>
		[Contracts.Pure]
		public int FindGroupIndexByTag(string tagString)
		{
			Contract.Requires(!string.IsNullOrEmpty(tagString));
			Contract.Requires(tagString.Length == this.NullGroupTag.Tag.Length,
				"Tag lengths mismatch");

			return this.BaseGroupTags.FindIndex(gt => gt.TagString == tagString);
		}

		/// <summary>Finds the index of a <see cref="GroupTagData"/></summary>
		/// <param name="groupName">The name of a <see cref="GroupTagData"/> to search for</param>
		/// <returns>Index of <paramref name="groupName"/> or <b>-1</b> if not found</returns>
		[Contracts.Pure]
		public int FindGroupIndex(string groupName)
		{
			Contract.Requires(!string.IsNullOrEmpty(groupName));

			return this.BaseGroupTags.FindIndex(gt => gt.Name == groupName);
		}

		/// <summary>Finds the index of a supplied <see cref="GroupTagData"/> object</summary>
		/// <param name="group"><see cref="GroupTagData"/> object whose index we want to find</param>
		/// <returns>Index of <paramref name="group"/> or <b>-1</b> if not found</returns>
		[Contracts.Pure]
		public int FindGroupIndex(GroupTagData group)
		{
			Contract.Requires(group != null);

			return this.BaseGroupTags.FindIndex(gt => ReferenceEquals(gt, group));
		}

		/// <summary>Finds a <see cref="GroupTagData"/> of this collection based on its group tag</summary>
		/// <param name="groupTag">The <see cref="GroupTagData"/>'s 'tag' to search for</param>
		/// <returns>Null if <paramref name="groupTag"/> isn't a part of this collection</returns>
		[Contracts.Pure]
		public GroupTagData FindGroup(char[] groupTag)
		{
			Contract.Requires(groupTag != null);

			int index = this.FindGroupIndexByTag(groupTag);
			if (index.IsNone())
				return null;

			return this.BaseGroupTags[index];
		}
		/// <summary>Finds a <see cref="GroupTagData"/> of this collection based on its group tag</summary>
		/// <param name="tagString">The <see cref="GroupTagData"/>'s 'tag' to search for</param>
		/// <returns>Null if <paramref name="tagString"/> isn't a part of this collection</returns>
		[Contracts.Pure]
		public GroupTagData FindGroupByTag(string tagString)
		{
			Contract.Requires(!string.IsNullOrEmpty(tagString));
			Contract.Requires(tagString.Length == this.NullGroupTag.Tag.Length,
				"Tag lengths mismatch");

			int index = this.FindGroupIndexByTag(tagString);
			if (index.IsNone())
				return null;

			return this.BaseGroupTags[index];
		}

		/// <summary>Finds a <see cref="GroupTagData"/> of this collection based on its group tag</summary>
		/// <param name="groupName">The name of a <see cref="GroupTagData"/> to search for</param>
		/// <returns>Null if <paramref name="groupName"/> isn't a part of this collection</returns>
		[Contracts.Pure]
		public GroupTagData FindGroup(string groupName)
		{
			Contract.Requires(!string.IsNullOrEmpty(groupName));

			int index = this.FindGroupIndex(groupName);
			if (index.IsNone())
				return null;

			return this.BaseGroupTags[index];
		}
		#endregion

		/// <summary>Sort the collection by the names of the group tags</summary>
		public void Sort() => Array.Sort<GroupTagData>(this.BaseGroupTags, new ByNameComparer());

		/// <summary>Does the collection contain multiple elements which share the same group tag and\or name?</summary>
		/// <returns></returns>
		[Contracts.Pure]
		public bool ContainsDuplicates()
		{
			var all_tags =	from gt in this.BaseGroupTags select gt.TagString;
			var all_names = from gt in this.BaseGroupTags select gt.Name;

			return all_tags.ContainsDuplicates() || all_names.ContainsDuplicates();
		}

		#region IEndianStreamable Members
		/// <summary>Seek ahead (<see cref="Count"/> * length of the <see cref="GroupTagData"/>'s character code) bytes</summary>
		/// <param name="s"></param>
		public abstract void Read(IO.EndianReader s);
		/// <summary>Writes each group tag's character code</summary>
		/// <param name="s"></param>
		public void Write(IO.EndianWriter s)
		{
			foreach (GroupTagData g in this.BaseGroupTags)
			{
				Contract.Assume(g != null);

				g.Write(s);
			}
		}
		#endregion

		#region IReadOnlyList<GroupTagData> Members
		public GroupTagData this[int index] { get => this.BaseGroupTags[index]; }
		public int Count { get => this.BaseGroupTags.Length; }
		#endregion

		#region IEnumerable<GroupTagData> Members
		public IEnumerator<GroupTagData> GetEnumerator() => (IEnumerator<GroupTagData>) this.BaseGroupTags.GetEnumerator();
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.BaseGroupTags.GetEnumerator();
		#endregion
	};
	[Contracts.ContractClassFor(typeof(GroupTagCollection))]
	abstract class GroupTagCollectionContract : GroupTagCollection
	{
		protected GroupTagCollectionContract() : base(null) => throw new NotImplementedException();

		protected override GroupTagData[] BaseGroupTags { get {
			Contract.Ensures(Contract.Result<GroupTagData[]>() != null);

			throw new NotImplementedException();
		} }

		public override GroupTagData NullGroupTag { get {
			Contract.Ensures(Contract.Result<GroupTagData>() != null);

			throw new NotImplementedException();
		} }
	};
}
