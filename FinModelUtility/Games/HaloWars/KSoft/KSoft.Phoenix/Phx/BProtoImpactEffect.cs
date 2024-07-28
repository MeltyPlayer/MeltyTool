
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoImpactEffect
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("TerrainEffect")
		{
			DataName = "name",
			Flags = 0
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.Data,
			FileName = "ImpactEffects.xml",
			RootName = "ImpactEffects"//kBListXmlParams.RootName
		};
		public static readonly Engine.ProtoDataXmlFileInfo kProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.Lists,
			kXmlFileInfo);
		#endregion

		#region Limit
		int mLimit = 2;
		public int Limit
		{
			get { return this.mLimit; }
			set { this.mLimit = value; }
		}
		#endregion

		#region Lifespan
		float mLifespan = 3.0f;
		public float Lifespan
		{
			get { return this.mLifespan; }
			set { this.mLifespan = value; }
		}
		#endregion

		#region FileName
		string mFileName;
		public string FileName
		{
			get { return this.mFileName; }
			set { this.mFileName = value; }
		}
		#endregion

		#region BListAutoIdObject Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttributeOpt("limit", ref this.mLimit, x => x != 2);
			s.StreamAttributeOpt("lifespan", ref this.mLifespan, x => x != 3.0f);
			s.StreamCursor(ref this.mFileName);
		}
		#endregion
	};
}