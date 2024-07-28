
namespace KSoft.Phoenix.Phx
{
	/// <summary>Script objects which can be "commented" out</summary>
	public abstract class TriggerScriptCodeObject
		: TriggerScriptIdObject
	{
		#region Xml constants
		const string kXmlAttrCommentOut = "CommentOut";
		#endregion

		bool mCommentOut;
		public bool CommentOut { get { return this.mCommentOut; } }

		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamAttribute(kXmlAttrCommentOut, ref this.mCommentOut);
		}
	};
}