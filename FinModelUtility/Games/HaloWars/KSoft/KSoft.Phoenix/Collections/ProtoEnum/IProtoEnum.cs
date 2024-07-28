using System;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Collections
{
	[Contracts.ContractClass(typeof(IProtoEnumContract))]
	public interface IProtoEnum
	{
		[Contracts.Pure]
		int TryGetMemberId(string memberName);
		[Contracts.Pure]
		string TryGetMemberName(int memberId);

		[Contracts.Pure]
		bool IsValidMemberId(int memberId);
		[Contracts.Pure]
		bool IsValidMemberName(string memberName);

		[Contracts.Pure]
		int GetMemberId(string memberName);
		[Contracts.Pure]
		string GetMemberName(int memberId);

		/// <summary>Number of members</summary>
		[Contracts.Pure]
		int MemberCount { get; }
	};

	[Contracts.ContractClassFor(typeof(IProtoEnum))]
	abstract class IProtoEnumContract
		: IProtoEnum
	{
		#region IProtoEnum Members
		int IProtoEnum.TryGetMemberId(string memberName)
		{
			Contract.Ensures(Contract.Result<int>().IsNoneOrPositive());

			throw new NotImplementedException();
		}
		public abstract string TryGetMemberName(int memberId);
		public abstract bool IsValidMemberId(int memberId);
		public abstract bool IsValidMemberName(string memberName);
		public abstract int GetMemberId(string memberName);
		string IProtoEnum.GetMemberName(int memberId)
		{
			Contract.Requires<ArgumentOutOfRangeException>(this.IsValidMemberId(memberId));

			throw new NotImplementedException();
		}

		int IProtoEnum.MemberCount { get {
			Contract.Ensures(Contract.Result<int>() >= 0);

			throw new NotImplementedException();
		} }
		#endregion
	};
}

namespace KSoft.Phoenix
{
	partial class TypeExtensionsPhx
	{
		public static int TryGetId(this Collections.IProtoEnum dbi, string name)
		{
			if (dbi == null)
				return TypeExtensions.kNone;

			return dbi.TryGetMemberId(name);
		}
	};
}