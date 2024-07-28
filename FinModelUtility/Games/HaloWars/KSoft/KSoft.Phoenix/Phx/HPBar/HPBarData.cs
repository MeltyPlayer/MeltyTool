using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Phx
{
	[ProtoDataTypeObjectSourceKind(ProtoDataObjectSourceKind.HPData)]
	public sealed class HPBarData
		: IO.ITagElementStringNameStreamable
		, IProtoDataObjectDatabaseProvider
	{
		public ProtoDataObjectDatabase ObjectDatabase { get; private set; }

		#region Xml constants
		const string kXmlRoot = "HPBarDefinition";

		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.Data,
			FileName = "HPBars.xml",
			RootName = kXmlRoot
		};
		public static readonly Engine.ProtoDataXmlFileInfo kProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.GameData,
			kXmlFileInfo);
		#endregion

		public Collections.BListAutoId<BProtoHPBar> HPBars { get; private set; }
		public Collections.BListAutoId<BProtoHPBarColorStages> ColorStages { get; private set; }
		public Collections.BListAutoId<BProtoVeterancyBar> VeterancyBars { get; private set; }
		public Collections.BListAutoId<BProtoPieProgress> PieProgress { get; private set; }
		public Collections.BListAutoId<BProtoBobbleHead> BobbleHeads { get; private set; }
		public Collections.BListAutoId<BProtoBuildingStrength> BuildingStrengths { get; private set; }

		public HPBarData()
		{
			this.ObjectDatabase = new ProtoDataObjectDatabase(this, typeof(HPBarDataObjectKind));

			this.HPBars = new Collections.BListAutoId<BProtoHPBar>();
			this.ColorStages = new Collections.BListAutoId<BProtoHPBarColorStages>();
			this.VeterancyBars = new Collections.BListAutoId<BProtoVeterancyBar>();
			this.PieProgress = new Collections.BListAutoId<BProtoPieProgress>();
			this.BobbleHeads = new Collections.BListAutoId<BProtoBobbleHead>();
			this.BuildingStrengths = new Collections.BListAutoId<BProtoBuildingStrength>();

			this.InitializeDatabaseInterfaces();
		}

		public void Clear()
		{
			this.HPBars.Clear();
			this.ColorStages.Clear();
			this.VeterancyBars.Clear();
			this.PieProgress.Clear();
			this.BobbleHeads.Clear();
			this.BuildingStrengths.Clear();
		}

		#region Database interfaces
		void InitializeDatabaseInterfaces()
		{
			this.HPBars.SetupDatabaseInterface();
			this.ColorStages.SetupDatabaseInterface();
			this.VeterancyBars.SetupDatabaseInterface();
			this.PieProgress.SetupDatabaseInterface();
			this.BobbleHeads.SetupDatabaseInterface();
			this.BuildingStrengths.SetupDatabaseInterface();
		}

		internal Collections.IBTypeNames GetNamesInterface(HPBarDataObjectKind kind)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != HPBarDataObjectKind.None);

			switch (kind)
			{
			case HPBarDataObjectKind.HPBar:            return this.HPBars;
			case HPBarDataObjectKind.ColorStages:      return this.ColorStages;
			case HPBarDataObjectKind.VeterancyBar:     return this.VeterancyBars;
			case HPBarDataObjectKind.PieProgress:      return this.PieProgress;
			case HPBarDataObjectKind.BobbleHead:       return this.BobbleHeads;
			case HPBarDataObjectKind.BuildingStrength: return this.BuildingStrengths;

			default: throw new KSoft.Debug.UnreachableException(kind.ToString());
			}
		}

		internal Collections.IHasUndefinedProtoMemberInterface GetMembersInterface(HPBarDataObjectKind kind)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != HPBarDataObjectKind.None);

			switch (kind)
			{
			case HPBarDataObjectKind.HPBar:            return this.HPBars;
			case HPBarDataObjectKind.ColorStages:      return this.ColorStages;
			case HPBarDataObjectKind.VeterancyBar:     return this.VeterancyBars;
			case HPBarDataObjectKind.PieProgress:      return this.PieProgress;
			case HPBarDataObjectKind.BobbleHead:       return this.BobbleHeads;
			case HPBarDataObjectKind.BuildingStrength: return this.BuildingStrengths;

			default: throw new KSoft.Debug.UnreachableException(kind.ToString());
			}
		}
		#endregion

		#region ITagElementStreamable<string> Members
		/// <remarks>For streaming directly from hpbars.xml</remarks>
		internal void StreamHPBarData<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			XML.XmlUtil.Serialize(s, this.HPBars, BProtoHPBar.kBListXmlParams);
			XML.XmlUtil.Serialize(s, this.ColorStages, BProtoHPBarColorStages.kBListXmlParams);
			XML.XmlUtil.Serialize(s, this.VeterancyBars, BProtoVeterancyBar.kBListXmlParams);
			XML.XmlUtil.Serialize(s, this.PieProgress, BProtoPieProgress.kBListXmlParams);
			XML.XmlUtil.Serialize(s, this.BobbleHeads, BProtoBobbleHead.kBListXmlParams);
			XML.XmlUtil.Serialize(s, this.BuildingStrengths, BProtoBuildingStrength.kBListXmlParams);
		}

		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterCursorBookmark(kXmlRoot))
				this.StreamHPBarData(s);
		}
		#endregion

		#region IProtoDataObjectDatabaseProvider members
		Engine.XmlFileInfo IProtoDataObjectDatabaseProvider.SourceFileReference { get { return kXmlFileInfo; } }

		Collections.IBTypeNames IProtoDataObjectDatabaseProvider.GetNamesInterface(int objectKind)
		{
			var kind = (HPBarDataObjectKind)objectKind;
			return this.GetNamesInterface(kind);
		}

		Collections.IHasUndefinedProtoMemberInterface IProtoDataObjectDatabaseProvider.GetMembersInterface(int objectKind)
		{
			var kind = (HPBarDataObjectKind)objectKind;
			return this.GetMembersInterface(kind);
		}
		#endregion
	};
}