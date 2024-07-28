using System;
using System.Collections.Generic;
using System.Linq;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using TagWord = System.UInt32;

namespace KSoft.Values
{
	using GroupTagDatum = GroupTagData32;

	/// <summary>Collection of 32-bit Group Tags</summary>
	public sealed class GroupTag32Collection : GroupTagCollection
	{
		#region Elements
		readonly GroupTagDatum[] mGroupTags;
		/// <summary>This collection's group tag elements</summary>
		public IReadOnlyList<GroupTagDatum> GroupTags { get { return this.mGroupTags; } }

		protected override GroupTagData[] BaseGroupTags { get { return (GroupTagData[]) this.mGroupTags; } }
		#endregion

		public override GroupTagData NullGroupTag { get { return GroupTagDatum.Null; } }

		#region Ctor
		/// <summary>Create a collection based on an existing list of group tags</summary>
		/// <param name="groupTags">Group tags to populate this collection with</param>
		public GroupTag32Collection(params GroupTagDatum[] groupTags) : this(KGuid.Empty, groupTags)
		{
			Contract.Requires<ArgumentNullException>(groupTags != null);
		}
		/// <summary>Create a collection based on an existing list of group tags and a <see cref="Guid"/></summary>
		/// <param name="uuid">Guid for this group tag collection</param>
		/// <param name="groupTags">Group tags to populate this collection with</param>
		public GroupTag32Collection(KGuid uuid, params GroupTagDatum[] groupTags) : base(groupTags, uuid)
		{
			Contract.Requires<ArgumentNullException>(groupTags != null);

			this.mGroupTags = new GroupTagDatum[groupTags.Length];
			groupTags.CopyTo(this.mGroupTags, 0);
		}
		/// <summary>Create a collection using an explicit list of group tags</summary>
		/// <param name="sort">Should we sort the list?</param>
		/// <param name="groupTags">Group tags to populate this collection with</param>
		public GroupTag32Collection(bool sort, params GroupTagDatum[] groupTags) : this(groupTags)
		{
			Contract.Requires<ArgumentNullException>(groupTags != null);

			if (sort)
				this.Sort();
		}
		/// <summary>Create a collection using an explicit list of group tags and a <see cref="Guid"/></summary>
		/// <param name="uuid">Guid for this group tag collection</param>
		/// <param name="sort">Should we sort the list?</param>
		/// <param name="groupTags">Group tags to populate this collection with</param>
		public GroupTag32Collection(KGuid uuid, bool sort, params GroupTagDatum[] groupTags) : this(uuid, groupTags)
		{
			Contract.Requires<ArgumentNullException>(groupTags != null);

			if (sort)
				this.Sort();
		}
		#endregion

		#region Searching
		/// <summary>Finds the index of a <see cref="GroupTagData32"/></summary>
		/// <param name="groupTag">The id of a group tag to search for</param>
		/// <returns>Index of <paramref name="group"/> or <b>-1</b> if not found</returns>
		[Contracts.Pure]
		public int FindGroupIndexByTag(TagWord groupTag)
		{
			return this.GroupTags.FindIndex(gt => gt.ID == groupTag);
		}

		/// <summary>Find a <see cref="GroupTagData32"/> object in this collection based on it's group tag</summary>
		/// <param name="tag">Group tag to find</param>
		/// <returns><see cref="GroupTagData32"/> object existing in this collection, or null if not found.</returns>
		[Contracts.Pure]
		public GroupTagDatum FindGroupByTag(TagWord tag)
		{
			var matching_tags = from gt in this.GroupTags
								where gt.ID == tag
								select gt;

			return matching_tags.FirstOrDefault();
		}

		/// <summary>Determines if a group tag ID exists in this collection</summary>
		/// <param name="tag">Group tag id to find</param>
		/// <returns></returns>
		[Contracts.Pure]
		public bool Contains(TagWord tag)
		{
			return this.GroupTags.Any(gt => gt.ID == tag);
		}
		#endregion

		#region IEndianStreamable Members
		/// <summary>Moves the stream ahead by the sizeof a four character code (4 bytes) times the count of the <see cref="GroupTags"/></summary>
		/// <param name="s"></param>
		/// <remarks>Doesn't actually read any data from the stream, only seeks forward</remarks>
		public override void Read(IO.EndianReader s)
		{
			s.Seek(this.GroupTags.Count * sizeof(TagWord), System.IO.SeekOrigin.Current);
		}
		#endregion

		/// <summary>Get a new instance of an empty collection</summary>
		public static GroupTag32Collection Empty { get {
			return new GroupTag32Collection();
		} }
	};


	/// <summary>Attribute applied to classes which house a static <see cref="GroupTag32Collection"/> property</summary>
	/// <remarks>
	/// Allows for ease-of-use in other attributes where we'd need to index a <see cref="GroupTag32Collection"/>
	/// collection for a specific <see cref="GroupTagData32"/> member.
	///
	/// <see cref="GroupTagContainerAttribute.kDefaultName"/> is the default name value used for the "main"
	/// collection lookup
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class GroupTagContainer32Attribute : GroupTagContainerAttribute
	{
		/// <summary>Initialize the attribute with a type containing a <see cref="GroupTag32Collection"/></summary>
		/// <param name="container">A type which contains static <see cref="GroupTag32Collection"/> properties</param>
		public GroupTagContainer32Attribute(Type container) : base(container)
		{
			Contract.Requires(container != null);
		}
		/// <summary>Initialize the attribute with a type containing a <see cref="GroupTag32Collection"/></summary>
		/// <param name="container">A type which contains static <see cref="GroupTag32Collection"/> properties</param>
		/// <param name="collectionName">Explicit name for the "main" group collection property</param>
		protected GroupTagContainer32Attribute(Type container, string collectionName) : base(container, collectionName)
		{
			Contract.Requires(container != null);
			Contract.Requires(!string.IsNullOrEmpty(collectionName));
		}

		/// <summary>The "main" group of the class which this attribute was applied to</summary>
		public GroupTag32Collection Collection { get { return this.TagCollection as GroupTag32Collection; } }
	};
}
