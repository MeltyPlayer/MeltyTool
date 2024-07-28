
namespace KSoft.Phoenix.Phx
{
	/// <summary>Explicitly ID'd script objects</summary>
	public abstract class TriggerScriptIdObject
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		const string kXmlAttrId = "ID"; // EditorID
		#endregion

		int mID = TypeExtensions.kNone;
		public int ID { get { return this.mID; } }

		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttribute(kXmlAttrId, ref this.mID);
		}
	};
}