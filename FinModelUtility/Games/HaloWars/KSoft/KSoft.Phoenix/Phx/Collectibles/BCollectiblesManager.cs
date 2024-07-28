
namespace KSoft.Phoenix.Phx
{
	public sealed class BCollectiblesManager
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public const string kXmlRootName = "CollectiblesDefinitions";

		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.Data,
			FileName = "Skulls.xml",
			RootName = kXmlRootName
		};
		#endregion

		int mXmlVersion = TypeExtensions.kNone;
		public BCollectiblesSkullManager SkullManager { get; private set; } = new BCollectiblesSkullManager();

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamElementOpt("CollectiblesXMLVersion", ref this.mXmlVersion, Predicates.IsNotNone);
			this.SkullManager.Serialize(s);
		}
		#endregion
	};
}
