
namespace KSoft.Phoenix.Phx
{
	public sealed class BWeaponType
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("WeaponType")
		{
			DataName = "Name",
			Flags = XML.BCollectionXmlParamsFlags.UseElementForData
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.Data,
			FileName = "WeaponTypes.xml",
			RootName = kBListXmlParams.RootName
		};
		public static readonly Engine.ProtoDataXmlFileInfo kProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.Lists,
			kXmlFileInfo);
		#endregion

		#region DeathAnimation
		string mDeathAnimation;
		public string DeathAnimation
		{
			get { return mDeathAnimation; }
			set { mDeathAnimation = value; }
		}
		#endregion

		public Collections.BTypeValues<BWeaponModifier> Modifiers { get; private set; } = new Collections.BTypeValues<BWeaponModifier>(BWeaponModifier.kBListParams);

		#region BListObjectBase Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamElementOpt("DeathAnimation", ref mDeathAnimation, Predicates.IsNotNullOrEmpty);

			XML.XmlUtil.Serialize(s, Modifiers, BWeaponModifier.kBListXmlParams);
		}
		#endregion
	};
}
