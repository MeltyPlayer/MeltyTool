
namespace KSoft.Phoenix.Phx
{
	public sealed class TerrainTileType
		: Collections.BListAutoIdObject
	{
		public const int cUndefinedIndex = 0;

		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("TerrainTileType")
		{
			DataName = "name",
			Flags = 0,
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.Data,
			FileName = "TerrainTileTypes.xml",
			RootName = "TerrainTileTypes"//kBListXmlParams.RootName
		};
		public static readonly Engine.ProtoDataXmlFileInfo kProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.Lists,
			kXmlFileInfo);
		#endregion

		#region EditorColor
		uint mEditorColor;
		public System.Drawing.Color EditorColor
		{
			get { return System.Drawing.Color.FromArgb((int) this.mEditorColor); }
			set { this.mEditorColor = (uint)value.ToArgb(); }
		}
		#endregion

		#region ImpactEffect
		string mImpactEffect;
		[Meta.UnusedData]
		[Meta.VisualReference]
		public string ImpactEffect
		{
			get { return this.mImpactEffect; }
			set { this.mImpactEffect = value; }
		}
		#endregion

		#region BListAutoIdObject Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttribute("EditorColor", ref this.mEditorColor, NumeralBase.Hex);
			s.StreamElementOpt("ImpactEffect", ref this.mImpactEffect, Predicates.IsNotNullOrEmpty);
		}
		#endregion
	};
}