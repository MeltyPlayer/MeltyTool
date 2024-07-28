using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.XML
{
	partial class XmlUtil
	{
		public static void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			Collections.BTypeValuesString list, BTypeValuesXmlParams<string> @params)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(s != null);
			Contract.Requires(list != null);
			Contract.Requires(@params != null);

			using(var xs = new BTypeValuesStringXmlSerializer(@params, list))
			{
				xs.Serialize(s);
			}
		}
	};

	internal sealed class BTypeValuesStringXmlSerializer
		: BTypeValuesXmlSerializerBase<string>
	{
		public BTypeValuesStringXmlSerializer(BTypeValuesXmlParams<string> @params, Collections.BTypeValuesString list) : base(@params, list)
		{
			Contract.Requires<ArgumentNullException>(@params != null);
			Contract.Requires<ArgumentNullException>(list != null);
		}

		#region IXmlElementStreamable Members
		protected override void Read<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, int iteration)
		{
			int index = this.ReadExplicitIndex(s, xs);

			this.ListExplicitIndex.InitializeItem(index);
			string value = null;
			s.ReadCursor(ref value);
			this.ListExplicitIndex[index] = value;
		}
		protected override void Write<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, string data)
		{
			s.WriteCursor(data);
		}
		#endregion
	};
}