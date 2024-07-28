
namespace KSoft.Phoenix.Phx
{
	public sealed class BHitZone
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "HitZone",
		};
		#endregion

		#region AttachmentName
		string mAttachmentName;
		public string AttachmentName
		{
			get { return this.mAttachmentName; }
			set { this.mAttachmentName = value; }
		}
		#endregion

		#region Hitpoints
		float mHitpoints = PhxUtil.kInvalidSingle;
		public float Hitpoints
		{
			get { return this.mHitpoints; }
			set { this.mHitpoints = value; }
		}
		#endregion

		#region Shieldpoints
		float mShieldpoints = PhxUtil.kInvalidSingle;
		public float Shieldpoints
		{
			get { return this.mShieldpoints; }
			set { this.mShieldpoints = value; }
		}
		#endregion

		#region Active
		float mActive;
		public float Active
		{
			get { return this.mActive; }
			set { this.mActive = value; }
		}
		#endregion

		#region HasShields
		float mHasShields;
		public float HasShields
		{
			get { return this.mHasShields; }
			set { this.mHasShields = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamCursor(ref this.mAttachmentName);
			s.StreamElementOpt("Hitpoints", ref this.mHitpoints, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("Shieldpoints", ref this.mShieldpoints, PhxPredicates.IsNotInvalid);
		}
		#endregion
	};
}