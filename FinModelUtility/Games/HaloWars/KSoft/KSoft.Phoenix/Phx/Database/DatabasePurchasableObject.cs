
namespace KSoft.Phoenix.Phx
{
	public abstract class DatabasePurchasableObject
		: DatabaseNamedObject
	{
		XML.BTypeValuesXmlParams<float> mResourceCostXmlParams;

		public Collections.BTypeValuesSingle ResourceCost { get; private set; }

		#region BuildTime
		float mBuildTime = PhxUtil.kInvalidSingle;
		public float BuildTime
		{
			get { return this.mBuildTime; }
			set { this.mBuildTime = value; }
		}
		#endregion

		#region ResearchTime
		float mResearchTime = PhxUtil.kInvalidSingle;
		public float ResearchTime
		{
			get { return this.mResearchTime; }
			set { this.mResearchTime = value; }
		}
		#endregion

		/// <summary>Time, in seconds, it takes to build or research this object</summary>
		public float PurchaseTime { get {
			return this.mBuildTime != PhxUtil.kInvalidSingle
				? this.mBuildTime
				: this.mResearchTime;
		} }

		protected DatabasePurchasableObject(Collections.BTypeValuesParams<float> rsrcCostParams, XML.BTypeValuesXmlParams<float> rsrcCostXmlParams)
		{
			this.mResourceCostXmlParams = rsrcCostXmlParams;

			this.ResourceCost = new Collections.BTypeValuesSingle(rsrcCostParams);
		}

		#region IXmlElementStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			XML.XmlUtil.Serialize(s, this.ResourceCost, this.mResourceCostXmlParams);
			s.StreamElementOpt("BuildPoints", ref this.mBuildTime, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("ResearchPoints", ref this.mResearchTime, PhxPredicates.IsNotInvalid);
		}
		#endregion
	};
}