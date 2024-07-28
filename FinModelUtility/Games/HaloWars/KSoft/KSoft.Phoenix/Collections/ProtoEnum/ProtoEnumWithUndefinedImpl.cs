using System.Collections.ObjectModel;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Collections
{
	using PhxUtil = Phoenix.PhxUtil;

	internal sealed class ProtoEnumWithUndefinedImpl
		: IProtoEnumWithUndefined
	{
		IProtoEnum mRoot;
		ObservableCollection<string> mUndefined;

		public ProtoEnumWithUndefinedImpl(IProtoEnum root)
		{
			Contract.Requires(root != null);

			this.mRoot = root;
		}

		void InitializeUndefined()
		{
			if (this.mUndefined == null)
				this.mUndefined = new ObservableCollection<string>();
		}

		public void Clear()
		{
			if (this.mUndefined != null)
				this.mUndefined.Clear();
		}

		#region IProtoEnum Members
		public int TryGetMemberId(string memberName)		{ return this.mRoot.TryGetMemberId(memberName); }
		public string TryGetMemberName(int memberId)		{ return this.mRoot.TryGetMemberName(memberId); }
		public bool IsValidMemberId(int memberId)			{ return this.mRoot.IsValidMemberId(memberId); }
		public bool IsValidMemberName(string memberName)	{ return this.mRoot.IsValidMemberName(memberName); }
		public int GetMemberId(string memberName)			{ return this.mRoot.GetMemberId(memberName); }
		public string GetMemberName(int memberId)			{ return this.mRoot.GetMemberName(memberId); }
		public int MemberCount						{ get	{ return this.mRoot.MemberCount; } }
		#endregion

		#region IProtoEnumWithUndefined Members
		public int TryGetMemberIdOrUndefined(string memberName)
		{
			int id = this.TryGetMemberId(memberName);

			if (id.IsNone() && this.MemberUndefinedCount != 0)
			{
				id = this.mUndefined.FindIndex(str => PhxUtil.StrEqualsIgnoreCase(str, memberName));
				if (id.IsNotNone())
					id = PhxUtil.GetUndefinedReferenceHandle(id);
			}

			return id;
		}

		public int GetMemberIdOrUndefined(string memberName)
		{
			int id = this.TryGetMemberIdOrUndefined(memberName);

			if (id.IsNone())
			{
				this.InitializeUndefined();

				id = this.mUndefined.Count;
				this.mUndefined.Add(memberName);
				id = PhxUtil.GetUndefinedReferenceHandle(id);
			}

			return id;
		}

		public string GetMemberNameOrUndefined(int memberId)
		{
			string name;

			if (PhxUtil.IsUndefinedReferenceHandle(memberId))
			{
				Contract.Assert(this.mUndefined != null);
				name = this.mUndefined[PhxUtil.GetUndefinedReferenceDataIndex(memberId)];
			}
			else
			{
				name = this.GetMemberName(memberId);
			}

			return name;
		}

		public int MemberUndefinedCount { get {
			return this.mUndefined != null
				? this.mUndefined.Count
				: 0;
		} }

		public ObservableCollection<string> UndefinedMembers { get { return this.mUndefined; } }
		#endregion
	};
}