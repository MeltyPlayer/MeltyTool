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
			Collections.BTypeValuesSingle list, BTypeValuesXmlParams<float> @params,
			string attrName = null)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(s != null);
			Contract.Requires(list != null);
			Contract.Requires(@params != null);

			BTypeValuesXmlSerializerBase<float> xs;
			if (attrName == null)	xs = new BTypeValuesSingleXmlSerializer(@params, list);
			else					xs = new BTypeValuesSingleAttrHackXmlSerializer(@params, list, attrName);

			using (xs)
			{
				xs.Serialize(s);
			}
		}
	};

	internal sealed class BTypeValuesSingleXmlSerializer
		: BTypeValuesXmlSerializerBase<float>
	{
		public BTypeValuesSingleXmlSerializer(BTypeValuesXmlParams<float> @params, Collections.BTypeValuesSingle list) : base(@params, list)
		{
			Contract.Requires<ArgumentNullException>(@params != null);
			Contract.Requires<ArgumentNullException>(list != null);
		}

		#region IXmlElementStreamable Members
		protected override void Read<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, int iteration)
		{
			int index = this.ReadExplicitIndex(s, xs);

			this.ListExplicitIndex.InitializeItem(index);
			float value = 0;
			s.ReadCursor(ref value);
			this.ListExplicitIndex[index] = value;
		}
		protected override void Write<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, float data)
		{
			s.WriteCursor(data);
		}
		#endregion
	};
}