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
		public static void SerializeCostHack<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			Collections.BTypeValuesSingle list)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(s != null);
			Contract.Requires(list != null);

			using(var xs = new BCostTypeValuesSingleAttrHackXmlSerializer(list))
			{
				xs.Serialize(s);
			}
		}
	};

	internal sealed class BCostTypeValuesSingleAttrHackXmlSerializer :
		BListExplicitIndexXmlSerializerBase<float>
	{
		// Just an alias for less typing and code
		static BTypeValuesXmlParams<float> kParams { get { return Phx.BResource.kBListTypeValuesXmlParams_Cost; } }

		Collections.BTypeValuesSingle mList;

		public override Collections.BListExplicitIndexBase<float> ListExplicitIndex { get { return this.mList; } }

		public BCostTypeValuesSingleAttrHackXmlSerializer(Collections.BTypeValuesSingle list) : base(kParams)
		{
			this.mList = list;
		}

		#region IXmlElementStreamable Members
		protected override void Read<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, int iteration) { throw new NotImplementedException(); }
		protected override void Write<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, float data) { throw new NotImplementedException(); }
		protected override int ReadExplicitIndex<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs) { throw new NotImplementedException(); }
		protected override void WriteExplicitIndex<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, int index) { throw new NotImplementedException(); }

		protected override void ReadNodes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs)
		{
			var penum = this.mList.TypeValuesParams.kGetProtoEnumFromDB(xs.Database);

			foreach (var attrName in s.AttributeNames)
			{
				// The only attributes in this are actual member names so we don't waste time calling
				// penum.IsValidMemberName only to call GetMemberId when we can just compare id to -1
				int index = penum.GetMemberId(attrName);
				if (index.IsNone()) continue;

				this.mList.InitializeItem(index);
				float value = PhxUtil.kInvalidSingle;
				s.ReadAttribute(attrName, ref value);
				this.mList[index] = value;
			}
		}
		protected override void WriteNodes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs)
		{
			var tvp = this.mList.TypeValuesParams;

			var penum = tvp.kGetProtoEnumFromDB(xs.Database);
			float k_invalid = tvp.kTypeGetInvalid();

			for (int x = 0; x < this.mList.Count; x++)
			{
				float data = this.mList[x];

				if (tvp.kComparer.Compare(data, k_invalid) != 0)
				{
					string name = penum.GetMemberName(x);
					s.WriteAttribute(name, data);
				}
			}
		}
		#endregion
	};
}