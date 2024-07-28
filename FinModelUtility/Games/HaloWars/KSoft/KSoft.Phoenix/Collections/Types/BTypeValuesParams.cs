using System;

namespace KSoft.Collections
{
	using Phx = Phoenix.Phx;

	public sealed class BTypeValuesParams<T>
		: BListExplicitIndexParams<T>
	{
		/// <summary>Get the source IProtoEnum from an engine's main database</summary>
		public readonly Func<Phx.BDatabaseBase, IProtoEnum> kGetProtoEnumFromDB;

		public BTypeValuesParams(Func<Phx.BDatabaseBase, IProtoEnum> protoEnumGetter)
		{
			this.kGetProtoEnumFromDB = protoEnumGetter;
		}
	};
}