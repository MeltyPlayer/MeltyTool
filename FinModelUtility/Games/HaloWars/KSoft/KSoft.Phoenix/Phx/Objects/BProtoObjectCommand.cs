namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoObjectCommand
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "Command",
		};
		#endregion

		#region Position
		int mPosition;
		public int Position
		{
			get { return mPosition; }
			set { mPosition = value; }
		}
		#endregion

		#region CommandType
		BProtoObjectCommandType mCommandType = BProtoObjectCommandType.Invalid;
		public BProtoObjectCommandType CommandType
		{
			get { return mCommandType; }
			set { mCommandType = value; }
		}
		#endregion

		#region ID
		int mID = TypeExtensions.kNone;
		public int ID
		{
			get { return mID; }
			set { mID = value; }
		}
		#endregion

		#region SquadMode
		BSquadMode mSquadMode = BSquadMode.Invalid;
		public BSquadMode SquadMode
		{
			get { return mSquadMode; }
			set { mSquadMode = value; }
		}
		#endregion

		#region AutoClose
		bool mAutoClose;
		public bool AutoClose
		{
			get { return mAutoClose; }
			set { mAutoClose = value; }
		}
		#endregion

		public bool IsValid { get {
			return CommandType != BProtoObjectCommandType.Invalid
				&& Position >= 0
				&& IsCommandDataValid;
		} }

		public bool IsCommandDataValid { get {
			if (CommandType.RequiresValidId())
				return ID.IsNotNone();

			if (CommandType == BProtoObjectCommandType.ChangeMode)
				return SquadMode != BSquadMode.Invalid;

			return true;
		} }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttribute("Position", ref mPosition);

			s.StreamAttributeEnum("Type", ref mCommandType);
			switch (mCommandType)
			{
			case BProtoObjectCommandType.Research: // proto tech
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref mID, DatabaseObjectKind.Tech, false, XML.XmlUtil.kSourceCursor);
				break;
			case BProtoObjectCommandType.TrainUnit: // proto object
			case BProtoObjectCommandType.Build:
			case BProtoObjectCommandType.BuildOther:
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref mID, DatabaseObjectKind.Object, false, XML.XmlUtil.kSourceCursor);
				break;
			case BProtoObjectCommandType.TrainSquad: // proto squad
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref mID, DatabaseObjectKind.Squad, false, XML.XmlUtil.kSourceCursor);
				break;

			case BProtoObjectCommandType.ChangeMode: // unused
				s.StreamCursorEnum(ref mSquadMode);
				break;

			case BProtoObjectCommandType.Ability:
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref mID, DatabaseObjectKind.Ability, false, XML.XmlUtil.kSourceCursor);
				break;
			case BProtoObjectCommandType.Power:
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref mID, DatabaseObjectKind.Power, false, XML.XmlUtil.kSourceCursor);
				break;
			}

			s.StreamAttributeOpt("AutoClose", ref mAutoClose, Predicates.IsTrue);
		}
		#endregion
	};
}
