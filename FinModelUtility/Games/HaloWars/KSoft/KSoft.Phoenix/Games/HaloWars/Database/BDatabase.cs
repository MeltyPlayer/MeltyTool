
namespace KSoft.Phoenix.HaloWars
{
	public sealed partial class BDatabase
		: Phx.BDatabaseBase
	{
		static readonly Collections.CodeEnum<BCodeObjectType> kGameObjectTypes = new Collections.CodeEnum<BCodeObjectType>();
		static readonly Collections.CodeEnum<BCodeProtoObject> kGameProtoObjectTypes = new Collections.CodeEnum<BCodeProtoObject>();
		static readonly Collections.CodeEnum<BScenarioWorld> kGameScenarioWorlds = new Collections.CodeEnum<BScenarioWorld>();

		public override Collections.IProtoEnum GameObjectTypes { get { return kGameObjectTypes; } }
		public override Collections.IProtoEnum GameProtoObjectTypes { get { return kGameProtoObjectTypes; } }
		public override Collections.IProtoEnum GameScenarioWorlds { get { return kGameScenarioWorlds; } }

		[Phx.Meta.BProtoPowerReference]
		public int RepairPowerID { get; private set; }
		[Phx.Meta.BProtoPowerReference]
		public int RallyPointPowerID { get; private set; }
		[Phx.Meta.BProtoPowerReference]
		public int HookRepairPowerID { get; private set; }
		[Phx.Meta.BProtoPowerReference]
		public int UnscOdstDropPowerID { get; private set; }

		public BDatabase(Engine.PhxEngine engine) : base(engine, kGameObjectTypes)
		{
			this.RepairPowerID = this.RallyPointPowerID = this.HookRepairPowerID = this.UnscOdstDropPowerID =
				TypeExtensions.kNone;
		}

		internal void SetupDBIDs()
		{
			this.RepairPowerID = this.GetId(Phx.DatabaseObjectKind.Power, "_Repair");
			this.RallyPointPowerID = this.GetId(Phx.DatabaseObjectKind.Power, "_RallyPoint");
			this.HookRepairPowerID = this.GetId(Phx.DatabaseObjectKind.Power, "HookRepair");
			this.UnscOdstDropPowerID = this.GetId(Phx.DatabaseObjectKind.Power, "UnscOdstDrop");
		}
	};
}