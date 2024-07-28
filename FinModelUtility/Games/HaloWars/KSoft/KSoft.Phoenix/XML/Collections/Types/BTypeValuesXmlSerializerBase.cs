using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.XML
{
	internal abstract class BTypeValuesXmlSerializerBase<T>
		: BListExplicitIndexXmlSerializerBase<T>
	{
		Collections.BTypeValuesBase<T> mList;

		public override Collections.BListExplicitIndexBase<T> ListExplicitIndex { get { return this.mList; } }

		protected BTypeValuesXmlSerializerBase(BTypeValuesXmlParams<T> @params, Collections.BTypeValuesBase<T> list) : base(@params)
		{
			Contract.Requires<ArgumentNullException>(@params != null);
			Contract.Requires<ArgumentNullException>(list != null);

			this.mList = list;
		}

		#region IXmlElementStreamable Members
		protected override int ReadExplicitIndex<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs)
		{
			string name = null;
			this.Params.StreamDataName(s, ref name);

			int index = this.mList.TypeValuesParams.kGetProtoEnumFromDB(xs.Database).GetMemberId(name);

			return index;
		}
		protected override void WriteExplicitIndex<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, int index)
		{
			string name = this.mList.TypeValuesParams.kGetProtoEnumFromDB(xs.Database).GetMemberName(index);

			this.Params.StreamDataName(s, ref name);
		}

		/// <summary>Not Implemented</summary>
		/// <exception cref="NotImplementedException" />
		protected override void Read<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, int iteration) { throw new NotImplementedException(); }
		/// <summary>Not Implemented</summary>
		/// <exception cref="NotImplementedException" />
		protected override void Write<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, T data) { throw new NotImplementedException(); }
		#endregion
	};
}