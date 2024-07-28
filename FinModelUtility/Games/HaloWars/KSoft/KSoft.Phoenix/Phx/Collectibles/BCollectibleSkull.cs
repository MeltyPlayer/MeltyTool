
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoSkull
		: DatabaseNamedObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "Skull",
			DataName = kXmlAttrName,
		};
		#endregion

		#region ObjectDBID
		int mObjectDBID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int ObjectDBID
		{
			get { return this.mObjectDBID; }
			set { this.mObjectDBID = value; }
		}
		#endregion

		public Collections.BListArray<BCollectibleSkullEffect> Effects { get; private set; }

		#region DisplayImageOn
		string mDisplayImageOn;
		[Meta.TextureReference]
		public string DisplayImageOn
		{
			get { return this.mDisplayImageOn; }
			set { this.mDisplayImageOn = value; }
		}
		#endregion

		#region DisplayImageOff
		string mDisplayImageOff;
		[Meta.TextureReference]
		public string DisplayImageOff
		{
			get { return this.mDisplayImageOff; }
			set { this.mDisplayImageOff = value; }
		}
		#endregion

		#region DisplayImageLocked
		string mDisplayImageLocked;
		[Meta.TextureReference]
		public string DisplayImageLocked
		{
			get { return this.mDisplayImageLocked; }
			set { this.mDisplayImageLocked = value; }
		}
		#endregion

		#region Hidden
		bool mHidden;
		public bool Hidden
		{
			get { return this.mHidden; }
			set { this.mHidden = value; }
		}
		#endregion

		public BProtoSkull()
		{
			var textData = this.CreateDatabaseObjectUserInterfaceTextData();
			textData.HasDescriptionID = true;
			textData.HasDisplayNameID = true;

			this.Effects = new Collections.BListArray<BCollectibleSkullEffect>();
		}

		#region ITagElementStreamable<string> Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttribute("objectdbid", ref this.mObjectDBID);
			XML.XmlUtil.Serialize(s, this.Effects, BCollectibleSkullEffect.kBListXmlParams);
			s.StreamElementOpt("DisplayImageOn", ref this.mDisplayImageOn, Predicates.IsNotNullOrEmpty);
			s.StreamElementOpt("DisplayImageOff", ref this.mDisplayImageOff, Predicates.IsNotNullOrEmpty);
			s.StreamElementOpt("DisplayImageLocked", ref this.mDisplayImageLocked, Predicates.IsNotNullOrEmpty);
			s.StreamElementNamedFlag("Hidden", ref this.mHidden);
		}
		#endregion
	};
}