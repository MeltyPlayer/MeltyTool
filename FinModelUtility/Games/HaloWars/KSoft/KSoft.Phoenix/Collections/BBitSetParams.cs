using System;

namespace KSoft.Collections
{
	using Phx = Phoenix.Phx;

	public sealed class BBitSetParams
	{
		/// <summary>Get the source IProtoEnum from a global object</summary>
		public readonly Func<IProtoEnum> kGetProtoEnum;
		/// <summary>Get the source IProtoEnum from an engine's main database</summary>
		public readonly Func<Phx.BDatabaseBase, IProtoEnum> kGetProtoEnumFromDB;
		public Func<int, bool> kGetMemberDefaultValue;

		public BBitSetParams(Func<Phx.BDatabaseBase, IProtoEnum> protoEnumGetter)
		{
			this.kGetProtoEnumFromDB = protoEnumGetter;
		}
		public BBitSetParams(Func<IProtoEnum> protoEnumGetter)
		{
			this.kGetProtoEnum = protoEnumGetter;
		}
	};
}