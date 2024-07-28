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
			Collections.BTypeNames list, BListXmlParams @params, bool forceNoRootElementStreaming = false)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(s != null);
			Contract.Requires(list != null);
			Contract.Requires(@params != null);

			if (forceNoRootElementStreaming) @params.SetForceNoRootElementStreaming(true);
			using(var xs = new BTypeNamesXmlSerializer(@params, list))
			{
				xs.Serialize(s);
			}
			if (forceNoRootElementStreaming) @params.SetForceNoRootElementStreaming(false);
		}
	};

	internal class BTypeNamesXmlSerializer
		: BListXmlSerializerBase<string>
	{
		BListXmlParams mParams;
		Collections.BTypeNames mList;

		public override BListXmlParams Params { get { return this.mParams; } }
		public override Collections.BListBase<string> List { get { return this.mList; } }

		public BTypeNamesXmlSerializer(BListXmlParams @params, Collections.BTypeNames list)
		{
			Contract.Requires<ArgumentNullException>(@params != null);
			Contract.Requires<ArgumentNullException>(list != null);

			this.mParams = @params;
			this.mList = list;
		}

		#region IXmlElementStreamable Members
		protected override void Read<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, int iteration)
		{
			string name = null;
			this.mParams.StreamDataName(s, ref name);

			this.mList.AddItem(name);
		}
		protected override void Write<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, string name)
		{
			this.mParams.StreamDataName(s, ref name);
		}

		protected override void WriteNodes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs)
		{
			base.WriteNodes(s, xs);

			ProtoEnumUndefinedMembers.Write(s, this.mParams, this.mList.UndefinedInterface);
		}
		#endregion
	};
}