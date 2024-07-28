using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.XML
{
	/// <summary>
	/// Lame hack for type value maps which store their type name in the InnerText and the value in a fucking attribute
	/// </summary>
	internal sealed class BTypeValuesSingleAttrHackXmlSerializer
		: BTypeValuesXmlSerializerBase<float>
	{
		/*readonly*/ string kAttrName;

		public BTypeValuesSingleAttrHackXmlSerializer(BTypeValuesXmlParams<float> @params, Collections.BTypeValuesSingle list, string attributeName)
			: base(@params, list)
		{
			Contract.Requires<ArgumentNullException>(@params != null);
			Contract.Requires<ArgumentNullException>(list != null);
			Contract.Requires<ArgumentNullException>(attributeName != null);

			this.kAttrName = attributeName;
		}

		#region IXmlElementStreamable Members
		protected override void Read<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, int iteration)
		{
			int index = this.ReadExplicitIndex(s, xs);

			this.ListExplicitIndex.InitializeItem(index);
			float value = 0;
			s.ReadAttribute(this.kAttrName, ref value);
			this.ListExplicitIndex[index] = value;
		}
		protected override void Write<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, float data)
		{
			s.WriteAttribute(this.kAttrName, data);
		}
		#endregion
	};
}