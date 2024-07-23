
namespace KSoft.Phoenix.Phx
{
	public abstract class TriggerScriptObjectWithArgs
		: TriggerScriptObject
	{
		public Collections.BListExplicitIndex<BTriggerArg> Args { get; private set; } = new Collections.BListExplicitIndex<BTriggerArg>(BTriggerArg.kBListExplicitIndexParams);

		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			XML.XmlUtil.Serialize(s, Args, BTriggerArg.kBListExplicitIndexXmlParams);
		}
	};
}
