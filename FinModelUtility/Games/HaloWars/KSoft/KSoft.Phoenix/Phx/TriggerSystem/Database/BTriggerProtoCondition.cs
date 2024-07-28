
namespace KSoft.Phoenix.Phx
{
	public sealed class BTriggerProtoCondition
		: TriggerSystemProtoObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("Condition")
		{
			DataName = DatabaseNamedObject.kXmlAttrNameN,
			Flags = 0,
		};

		const string kXmlAttrAsync = "Async";
		const string kXmlAttrAsyncParameterKey = "AsyncParameterKey"; // really a sbyte
		#endregion

		bool mAsync;
		public bool Async { get { return this.mAsync; } }

		int mAsyncParameterKey;
		public int AsyncParameterKey { get { return this.mAsyncParameterKey; } }

		public BTriggerProtoCondition() { }
		public BTriggerProtoCondition(BTriggerSystem root, BTriggerCondition instance) : base(root, instance)
		{
			this.mAsync = instance.Async;
			this.mAsyncParameterKey = instance.AsyncParameterKey;
		}

		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			if(s.StreamAttributeOpt(kXmlAttrAsync, ref this.mAsync, Predicates.IsTrue))
				s.StreamAttribute(kXmlAttrAsyncParameterKey, ref this.mAsyncParameterKey);
		}
	};
}