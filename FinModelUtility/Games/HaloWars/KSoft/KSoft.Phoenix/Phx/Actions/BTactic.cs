using System.Collections.Generic;

using BProtoActionID = System.Int32;

namespace KSoft.Phoenix.Phx
{
	public sealed class BTactic
		: IO.ITagElementStringNameStreamable
	{

	#region Xml constants
	#endregion

	public Collections.BListArray<BTacticTargetRule> TargetRules { get; private set; } = new Collections.BListArray<BTacticTargetRule>();
		public List<BProtoActionID> PersistentActions { get; private set; } = new List<BProtoActionID>();
		public List<BProtoActionID> PersistentSquadActions { get; private set; } = new List<BProtoActionID>();

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var td = KSoft.Debug.TypeCheck.CastReference<BTacticData>(s.UserData);

			XML.XmlUtil.Serialize(s, this.TargetRules, BTacticTargetRule.kBListXmlParams);
			s.StreamElements("PersistentAction", this.PersistentActions, td, BTacticData.StreamProtoActionID);
			s.StreamElements("PersistentSquadAction", this.PersistentSquadActions, td, BTacticData.StreamProtoActionID);
		}
		#endregion
	};
}
