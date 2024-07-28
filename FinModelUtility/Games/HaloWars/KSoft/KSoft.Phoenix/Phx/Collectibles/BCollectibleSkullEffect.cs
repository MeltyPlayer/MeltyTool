
namespace KSoft.Phoenix.Phx
{
	//BSkullModifier
	public sealed class BCollectibleSkullEffect
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "Effect",
		};
		#endregion

		BCollectibleSkullEffectType mType = BCollectibleSkullEffectType.Invalid;
		BCollectibleSkullTarget mTarget = BCollectibleSkullTarget.None;
		float mValue = PhxUtil.kInvalidSingle;

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamCursorEnum(ref this.mType);
			s.StreamAttributeEnumOpt("target", ref this.mTarget, e => e != BCollectibleSkullTarget.None);
			s.StreamAttributeOpt("value", ref this.mValue, PhxPredicates.IsNotInvalid);
		}
		#endregion
	};
}