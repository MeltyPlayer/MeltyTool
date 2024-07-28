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
			get { return this.mPosition; }
			set { this.mPosition = value; }
		}
		#endregion

		#region CommandType
		BProtoObjectCommandType mCommandType = BProtoObjectCommandType.Invalid;
		public BProtoObjectCommandType CommandType
		{
			get { return this.mCommandType; }
			set { this.mCommandType = value; }
		}
		#endregion

		#region ID
		int mID = TypeExtensions.kNone;
		public int ID
		{
			get { return this.mID; }
			set { this.mID = value; }
		}
		#endregion

		#region SquadMode
		BSquadMode mSquadMode = BSquadMode.Invalid;
		public BSquadMode SquadMode
		{
			get { return this.mSquadMode; }
			set { this.mSquadMode = value; }
		}
		#endregion

		#region AutoClose
		bool mAutoClose;
		public bool AutoClose
		{
			get { return this.mAutoClose; }
			set { this.mAutoClose = value; }
		}
		#endregion

		public bool IsValid { get {
			return this.CommandType != BProtoObjectCommandType.Invalid
				&&
				this.Position >= 0
				&&
				this.IsCommandDataValid;
		} }

		public bool IsCommandDataValid { get {
			if (this.CommandType.RequiresValidId())
				return this.ID.IsNotNone();

			if (this.CommandType == BProtoObjectCommandType.ChangeMode)
				return this.SquadMode != BSquadMode.Invalid;

			return true;
		} }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttribute("Position", ref this.mPosition);

			s.StreamAttributeEnum("Type", ref this.mCommandType);
			switch (this.mCommandType)
			{
			case BProtoObjectCommandType.Research: // proto tech
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref this.mID, DatabaseObjectKind.Tech, false, XML.XmlUtil.kSourceCursor);
				break;
			case BProtoObjectCommandType.TrainUnit: // proto object
			case BProtoObjectCommandType.Build:
			case BProtoObjectCommandType.BuildOther:
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref this.mID, DatabaseObjectKind.Object, false, XML.XmlUtil.kSourceCursor);
				break;
			case BProtoObjectCommandType.TrainSquad: // proto squad
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref this.mID, DatabaseObjectKind.Squad, false, XML.XmlUtil.kSourceCursor);
				break;

			case BProtoObjectCommandType.ChangeMode: // unused
				s.StreamCursorEnum(ref this.mSquadMode);
				break;

			case BProtoObjectCommandType.Ability:
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref this.mID, DatabaseObjectKind.Ability, false, XML.XmlUtil.kSourceCursor);
				break;
			case BProtoObjectCommandType.Power:
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref this.mID, DatabaseObjectKind.Power, false, XML.XmlUtil.kSourceCursor);
				break;
			}

			s.StreamAttributeOpt("AutoClose", ref this.mAutoClose, Predicates.IsTrue);
		}
		#endregion
	};
}
