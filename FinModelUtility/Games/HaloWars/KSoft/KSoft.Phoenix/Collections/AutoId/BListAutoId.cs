using System;
using System.Collections.Generic;

namespace KSoft.Collections
{
	using PhxUtil = Phoenix.PhxUtil;

	public sealed class BListAutoIdParams
		: BListParams
	{
	};

	public sealed class BListAutoId<T>
		: BListBase<T>
		, IBTypeNames
		// For now, I don't see a reason to support struct types in AutoIds
		// If structs are needed, the streaming logic will need to be adjusted
		where T : class, IListAutoIdObject, new()
	{
		readonly string kUnregisteredMessage;

		static string BuildUnRegisteredMsg()
		{
			return string.Format("Unregistered {0}!", typeof(T).Name);
		}
		public BListAutoId(BListAutoIdParams @params = null) : base(@params)
		{
			this.kUnregisteredMessage = BuildUnRegisteredMsg();
			this.mUndefinedInterface = new ProtoEnumWithUndefinedImpl(this);
		}

		public override void Clear()
		{
			base.Clear();

			if (this.mDBI != null)
				this.mDBI.Clear();

			if (this.mUndefinedInterface != null)
				this.mUndefinedInterface.Clear();
		}

		#region Database interfaces
		/// <remarks>Mainly a hack for adding new items dynamically</remarks>
		void PreAdd(T item, string itemName, int id = TypeExtensions.kNone)
		{
			item.AutoId = id.IsNotNone()
				? id
				: this.Count;

			if (itemName != null)
				item.Data = itemName;
		}
		internal int DynamicAdd(T item, string itemName, int id = TypeExtensions.kNone)
		{
			this.PreAdd(item, itemName, id);
			if (this.mDBI != null)
			{
				if (this.mDBI.ContainsKey(itemName))
				{
					throw new ArgumentException(string.Format(
						"There is already a {0} named {1}",
						typeof(T).Name, itemName
						), nameof(itemName));
				}

				this.mDBI.Add(item.Data, item);

				if (this.Params != null && this.Params.ToLowerDataNames)
				{
					string lower_name = PhxUtil.ToLowerIfContainsUppercase(item.Data);
					if (!ReferenceEquals(lower_name, item.Data))
						this.mDBI.Add(lower_name, item);
				}
			}
			this.AddItem(item);

			return item.AutoId;
		}

		Dictionary<string, T> mDBI;
		internal void SetupDatabaseInterface()
		{
			this.mDBI = new Dictionary<string, T>(this.Params != null ? this.Params.InitialCapacity : BCollectionParams.kDefaultCapacity);
		}

		internal int TryGetId(string name)
		{
			int id = TypeExtensions.kNone;
			if (this.mDBI == null)
				return id;

			T obj;
			if (this.mDBI.TryGetValue(name, out obj))
				id = obj.AutoId;

			return id;
		}
		#endregion

		#region IProtoEnum Members
		public int TryGetMemberId(string memberName)
		{
			return this.mList.FindIndex(n => PhxUtil.StrEqualsIgnoreCase(n.Data, memberName));
		}
		public string TryGetMemberName(int memberId)
		{
			return this.IsValidMemberId(memberId) ? this.GetMemberName(memberId) : null;
		}
		public bool IsValidMemberId(int memberId)
		{
			return memberId >= 0 && memberId < this.Count;
		}
		public bool IsValidMemberName(string memberName)
		{
			int index = this.TryGetMemberId(memberName);

			return index.IsNotNone();
		}

		public int GetMemberId(string memberName)
		{
			int index = this.TryGetMemberId(memberName);

			if (index.IsNone())
				throw new ArgumentException(this.kUnregisteredMessage, memberName);

			return index;
		}
		public string GetMemberName(int memberId)
		{
			return this[memberId].Data;
		}

		public int MemberCount { get { return this.Count; } }
		#endregion

		public override object GetObject(int id)
		{
			if (id.IsNone())
				return null;

			if (PhxUtil.IsUndefinedReferenceHandle(id))
				return Phoenix.TypeExtensionsPhx.GetUndefinedObject(this.mUndefinedInterface, id);

			return base.GetObject(id);
		}

		private ProtoEnumWithUndefinedImpl mUndefinedInterface;
		IProtoEnumWithUndefined IHasUndefinedProtoMemberInterface.UndefinedInterface { get { return this.mUndefinedInterface; } }
		internal IProtoEnumWithUndefined UndefinedInterface { get { return this.mUndefinedInterface; } }
	};
}

namespace KSoft.Phoenix
{
	partial class TypeExtensionsPhx
	{
		internal static string TryGetName<T>(this Collections.BListAutoId<T> dbi, int id)
			where T : class, Collections.IListAutoIdObject, new()
		{
			if (dbi == null)
				return null;

			if (id >= 0 && id < dbi.Count)
				return dbi[id].Data;

			return null;
		}
	};
}