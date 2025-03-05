using System;

namespace KSoft.Collections
{
	using PhxUtil = Phoenix.PhxUtil;

	public interface IBTypeNames
		: IBList
		, IProtoEnum
		, IHasUndefinedProtoMemberInterface;

	public class BTypeNames
		: BListBase<string>
		, IBTypeNames
	{
		readonly string kUnregisteredMessage;

		static string BuildUnRegisteredMsg()
		{
			return string.Format("Unregistered {0}!", "BTypeName");
		}
		public BTypeNames()
		{
			this.kUnregisteredMessage = BuildUnRegisteredMsg();
			this.mUndefinedInterface = new ProtoEnumWithUndefinedImpl(this);
		}

		public override void Clear()
		{
			base.Clear();

			if (this.mUndefinedInterface != null)
				this.mUndefinedInterface.Clear();
		}

		#region IProtoEnum Members
		public virtual int TryGetMemberId(string memberName)
		{
			return this.mList.FindIndex(n => PhxUtil.StrEqualsIgnoreCase(n, memberName));
		}
		public virtual string TryGetMemberName(int memberId)
		{
			return this.IsValidMemberId(memberId)
				? this.GetMemberName(memberId)
				: null;
		}
		public bool IsValidMemberId(int memberId)
		{
			return memberId >= 0 && memberId < this.MemberCount;
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
		public virtual string GetMemberName(int memberId)
		{
			return this[memberId];
		}

		public virtual int MemberCount { get { return this.Count; } }
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
		public static string TryGetName(this Collections.BTypeNames dbi, int id)
		{
			if (dbi == null)
				return null;

			if (id >= 0 && id < dbi.Count)
				return dbi[id];

			return null;
		}
	};
}
