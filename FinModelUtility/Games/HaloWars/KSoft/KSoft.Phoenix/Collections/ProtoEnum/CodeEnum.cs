using System;

namespace KSoft.Collections
{
	using PhxUtil = Phoenix.PhxUtil;

	public sealed class CodeEnum<TEnum>
		: IProtoEnum
		where TEnum : struct
	{
		static readonly string[] kNames;
		static readonly string kUnregisteredMessage;

		static CodeEnum()
		{
			var enum_type = typeof(TEnum);

			kNames = Enum.GetNames(enum_type);

			kUnregisteredMessage = string.Format("Unregistered {0}!", enum_type.Name);
		}

		#region IProtoEnum Members
		public int TryGetMemberId(string memberName)
		{
			return Array.FindIndex(kNames, n => PhxUtil.StrEqualsIgnoreCase(n, memberName));
		}
		public string TryGetMemberName(int memberId)
		{
			return this.IsValidMemberId(memberId)
				? this.GetMemberName(memberId)
				: null;
		}
		public bool IsValidMemberId(int memberId)
		{
			return memberId >= 0 && memberId < kNames.Length;
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
				throw new ArgumentException(kUnregisteredMessage, memberName);

			return index;
		}
		public string GetMemberName(int memberId)
		{
			return kNames[memberId];
		}

		public int MemberCount { get { return kNames.Length; } }
		#endregion
	};
}