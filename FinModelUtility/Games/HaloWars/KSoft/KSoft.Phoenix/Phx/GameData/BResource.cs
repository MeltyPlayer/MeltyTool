
namespace KSoft.Phoenix.Phx
{
	public sealed class BResource
		: Collections.BListAutoIdObject
	{
		// fucking squads.xml and techs.xml uses a lower-case type name :|
		const bool kUseLowercaseCostTypeHack = true;

		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("Resource",
			additionalFlags: XML.BCollectionXmlParamsFlags.DoNotWriteUndefinedData);

		public static readonly Collections.BTypeValuesParams<float> kBListTypeValuesParams = new
			Collections.BTypeValuesParams<float>(db => db.GameData.Resources) { kTypeGetInvalid = PhxUtil.kGetInvalidSingle };
		public static readonly XML.BTypeValuesXmlParams<float> kBListTypeValuesXmlParams = new
			XML.BTypeValuesXmlParams<float>("Resource", "Type");
		public static readonly XML.BTypeValuesXmlParams<float> kBListTypeValuesXmlParams_Cost = new
			XML.BTypeValuesXmlParams<float>("Cost", "ResourceType");
#pragma warning disable 0429
		public static readonly XML.BTypeValuesXmlParams<float> kBListTypeValuesXmlParams_CostLowercaseType = !kUseLowercaseCostTypeHack
			? kBListTypeValuesXmlParams_Cost
			: new XML.BTypeValuesXmlParams<float>("Cost", "ResourceType".ToLowerInvariant()
			);
#pragma warning restore 0429
		public static readonly XML.BTypeValuesXmlParams<float> kBListTypeValuesXmlParams_AddResource = new
			XML.BTypeValuesXmlParams<float>("AddResource", null, XML.BCollectionXmlParamsFlags.UseInnerTextForData);
		#endregion

		bool mDeductable;
		public bool Deductable
		{
			get { return this.mDeductable; }
			set { this.mDeductable = value; }
		}

		public BResource() { }
		internal BResource(bool deductable) {
			this.mDeductable = deductable; }

		#region BListAutoIdObject Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttribute("Deductable", ref this.mDeductable);
		}
		#endregion
	};
}