using System;
using System.Threading.Tasks;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.Phx
{
	public enum DatabaseLoadState
	{
		NotLoaded,
		Failed,
		Preloading,
		Preloaded,
		Loading,
		Loaded,

		kNumberOf
	};

	[ProtoDataTypeObjectSourceKind(ProtoDataObjectSourceKind.Database)]
	public abstract partial class BDatabaseBase
		: ObjectModel.BasicViewModel
		, IDisposable
		, IO.ITagElementStringNameStreamable
		, IProtoDataObjectDatabaseProvider
	{
		public const string kInvalidString = "BORK BORK BORK";

		public ProtoDataObjectDatabase ObjectDatabase { get; private set; }

		#region Xml constants
		internal static readonly XML.BListXmlParams kObjectTypesXmlParams = new XML.BListXmlParams("ObjectType");
		internal static readonly Engine.XmlFileInfo kObjectTypesXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Phoenix.Engine.GameDirectory.Data,
			FileName = "ObjectTypes.xml",
			RootName = kObjectTypesXmlParams.RootName
		};
		internal static readonly Engine.ProtoDataXmlFileInfo kObjectTypesProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Phoenix.Engine.XmlFilePriority.Lists,
			kObjectTypesXmlFileInfo);
		#endregion

		#region LoadState
		DatabaseLoadState mLoadState = DatabaseLoadState.NotLoaded;
		public DatabaseLoadState LoadState
		{
			get
			{
				lock (this.mLoadStateLockee)
					return this.mLoadState;
			}
			set
			{
				lock (this.mLoadStateLockee)
					this.SetFieldEnum(ref this.mLoadState, value);
			}
		}

		object mLoadStateLockee = new object();
		#endregion

		public Engine.PhxEngine Engine { get; private set; }

		public abstract Collections.IProtoEnum GameObjectTypes { get; }
		public abstract Collections.IProtoEnum GameProtoObjectTypes { get; }
		public abstract Collections.IProtoEnum GameScenarioWorlds { get; }

		#region StringTable stuff
		public LocStringTable EnglishStringTable { get; private set; }
			= new LocStringTable();

		/// <summary>Maps a ID to a bit representing if any data references it somewhere. Only updated on data load</summary>
		public Collections.BitSet ReferencedStringIds { get; private set; }
			= new Collections.BitSet(ushort.MaxValue, fixedLength: false);

		internal void AddStringIDReference(int id)
		{
			if (this.ReferencedStringIds.Length < id)
			{
				int bump = id - this.ReferencedStringIds.Length;
				this.ReferencedStringIds.Length += bump + 16;
			}

			this.ReferencedStringIds[id] = true;
		}
		#endregion

		public BGameData GameData { get; private set; }
			 = new BGameData();
		public HPBarData HPBars { get; private set; }
			 = new HPBarData();

		#region DatabaseObjectKind lists
		// #NOTE place new DatabaseObjectKind code here
		public Collections.BListAutoId<		BDamageType> DamageTypes { get; private set; }
			= new Collections.BListAutoId<	BDamageType>();
		public Collections.BListAutoId<		BProtoImpactEffect> ImpactEffects { get; private set; }
			= new Collections.BListAutoId<	BProtoImpactEffect>();
		public Collections.BListAutoId<		BWeaponType> WeaponTypes { get; private set; }
			= new Collections.BListAutoId<	BWeaponType>();
		public Collections.BListAutoId<		BUserClass> UserClasses { get; private set; }
			= new Collections.BListAutoId<	BUserClass>();
		public Collections.BTypeNamesWithCode ObjectTypes { get; private set; }
		public Collections.BListAutoId<		BAbility> Abilities { get; private set; }
			 = new Collections.BListAutoId<	BAbility>();
		public Collections.BListAutoId<		BProtoObject> Objects { get; private set; }
			= new Collections.BListAutoId<	BProtoObject>(BProtoObject.kBListParams);
		public Collections.BListAutoId<		BProtoSquad> Squads { get; private set; }
			= new Collections.BListAutoId<	BProtoSquad>(BProtoSquad.kBListParams);
		public Collections.BListAutoId<		BProtoPower> Powers { get; private set; }
			= new Collections.BListAutoId<	BProtoPower>();
		public Collections.BListAutoId<		BTacticData> Tactics { get; private set; }
			= new Collections.BListAutoId<	BTacticData>();
		public Collections.BListAutoId<		BProtoTech> Techs { get; private set; }
			= new Collections.BListAutoId<	BProtoTech>(BProtoTech.kBListParams);
		public Collections.BListAutoId<		TerrainTileType> TerrainTileTypes { get; private set; }
			= new Collections.BListAutoId<	TerrainTileType>();
		public Collections.BListAutoId<		BCiv> Civs { get; private set; }
			= new Collections.BListAutoId<	BCiv>();
		public Collections.BListAutoId<		BLeader> Leaders { get; private set; }
			= new Collections.BListAutoId<	BLeader>();
		#endregion

		public Collections.BListArray<		BProtoMergedSquads> MergedSquads { get; private set; }
			= new Collections.BListArray<	BProtoMergedSquads>();
		public BProtoShieldBubbleTypes ShieldBubbleTypes { get; private set; }
			= new BProtoShieldBubbleTypes();

		protected BDatabaseBase(Engine.PhxEngine engine, Collections.IProtoEnum gameObjectTypes)
		{
			this.Engine = engine;

			this.ObjectDatabase = new ProtoDataObjectDatabase(this, typeof(DatabaseObjectKind));

			this.ObjectTypes = new Collections.BTypeNamesWithCode(gameObjectTypes);

			this.InitializeDatabaseInterfaces();
		}

		#region IDisposable Members
		public virtual void Dispose()
		{
			Util.DisposeAndNull(ref this.mTriggerSerializer);
		}
		#endregion

		#region Database interfaces
		// #NOTE place new DatabaseObjectKind code here

		void InitializeDatabaseInterfaces()
		{
			this.DamageTypes.SetupDatabaseInterface();
			this.ImpactEffects.SetupDatabaseInterface();
			this.WeaponTypes.SetupDatabaseInterface();
			this.UserClasses.SetupDatabaseInterface();
			this.Abilities.SetupDatabaseInterface();
			this.Objects.SetupDatabaseInterface();
			this.Squads.SetupDatabaseInterface();
			this.Tactics.SetupDatabaseInterface();
			this.Techs.SetupDatabaseInterface();
			this.TerrainTileTypes.SetupDatabaseInterface();
			this.Powers.SetupDatabaseInterface();
			this.Civs.SetupDatabaseInterface();
			this.Leaders.SetupDatabaseInterface();
		}

		const int kObjectIdIsObjectTypeBitMask = 1<<30;
		static void ObjectIdIsObjectTypeBitSet(ref int id)
		{
			id |= kObjectIdIsObjectTypeBitMask;
		}
		static bool ObjectIdIsObjectTypeBitGet(ref int id)
		{
			if ((id & kObjectIdIsObjectTypeBitMask) != 0)
			{
				id &= ~kObjectIdIsObjectTypeBitMask;
				return true;
			}
			return false;
		}

		int TryGetIdUnit(string name)
		{
			int id = this.Objects.TryGetId/*WithUndefined*/(name);

			if (id.IsNone())
			{
				if ((id = this.ObjectTypes.TryGetId(name)).IsNotNone())
					ObjectIdIsObjectTypeBitSet(ref id);
				else
					id = this.Objects.TryGetIdWithUndefined(name);
			}

			return id;
		}
		string TryGetNameUnit(int id)
		{
			if (ObjectIdIsObjectTypeBitGet(ref id))
				return this.ObjectTypes.TryGetNameWithUndefined(id);

			return this.Objects.TryGetNameWithUndefined(id);
		}

		public Collections.IBTypeNames GetNamesInterface(GameDataObjectKind kind)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != GameDataObjectKind.None);

			return this.GameData.GetNamesInterface(kind);
		}
		public Collections.IBTypeNames GetNamesInterface(HPBarDataObjectKind kind)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != HPBarDataObjectKind.None);

			return this.HPBars.GetNamesInterface(kind);
		}
		public Collections.IBTypeNames GetNamesInterface(DatabaseObjectKind kind)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != DatabaseObjectKind.None);

			// #NOTE place new DatabaseObjectKind code here

			switch (kind)
			{
			case DatabaseObjectKind.Ability:         return this.Abilities;
			case DatabaseObjectKind.Civ:             return this.Civs;
			case DatabaseObjectKind.DamageType:      return this.DamageTypes;
			case DatabaseObjectKind.ImpactEffect:    return this.ImpactEffects;
			case DatabaseObjectKind.Leader:          return this.Leaders;
			case DatabaseObjectKind.Object:          return this.Objects;
			case DatabaseObjectKind.ObjectType:      return this.ObjectTypes;
			case DatabaseObjectKind.Power:           return this.Powers;
			case DatabaseObjectKind.Squad:           return this.Squads;
			case DatabaseObjectKind.Tactic:          return this.Tactics;
			case DatabaseObjectKind.Tech:            return this.Techs;
			case DatabaseObjectKind.TerrainTileType: return this.TerrainTileTypes;
			case DatabaseObjectKind.Unit:            return null; // #TODO?
			case DatabaseObjectKind.UserClass:       return this.UserClasses;
			case DatabaseObjectKind.WeaponType:      return this.WeaponTypes;

			default: throw new KSoft.Debug.UnreachableException(kind.ToString());
			}
		}

		public int GetId(GameDataObjectKind kind, string name)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != GameDataObjectKind.None);

			var dbi = this.GameData.GetMembersInterface(kind);
			return dbi.TryGetIdWithUndefined(name);
		}
		public int GetId(HPBarDataObjectKind kind, string name)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != HPBarDataObjectKind.None);

			var dbi = this.HPBars.GetMembersInterface(kind);
			return dbi.TryGetIdWithUndefined(name);
		}
		public int GetId(DatabaseObjectKind kind, string name)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != DatabaseObjectKind.None);

			// #NOTE place new DatabaseObjectKind code here

			switch (kind)
			{
			case DatabaseObjectKind.Ability:         return this.Abilities.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.Civ:             return this.Civs.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.DamageType:      return this.DamageTypes.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.ImpactEffect:    return this.ImpactEffects.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.Leader:          return this.Leaders.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.Object:          return this.Objects.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.ObjectType:      return this.ObjectTypes.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.Power:           return this.Powers.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.Squad:           return this.Squads.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.Tactic:          return this.Tactics.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.Tech:            return this.Techs.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.TerrainTileType: return this.TerrainTileTypes.TryGetIdWithUndefined(name);
			// TODO: Should just use the Objects DBI AFAICT
			case DatabaseObjectKind.Unit:       return this.TryGetIdUnit(name);
			case DatabaseObjectKind.UserClass:  return this.UserClasses.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.WeaponType: return this.WeaponTypes.TryGetIdWithUndefined(name);

			default: throw new KSoft.Debug.UnreachableException(kind.ToString());
			}
		}
		public string GetName(GameDataObjectKind kind, int id)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != GameDataObjectKind.None);

			IProtoDataObjectDatabaseProvider provider = this.GameData;
			return provider.GetName((int)kind, id);
		}
		public string GetName(HPBarDataObjectKind kind, int id)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != HPBarDataObjectKind.None);

			IProtoDataObjectDatabaseProvider provider = this.HPBars;
			return provider.GetName((int)kind, id);
		}
		public string GetName(DatabaseObjectKind kind, int id)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != DatabaseObjectKind.None);

			// #NOTE place new DatabaseObjectKind code here

			switch (kind)
			{
			case DatabaseObjectKind.Ability:         return this.Abilities.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.Civ:             return this.Civs.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.DamageType:      return this.DamageTypes.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.ImpactEffect:    return this.ImpactEffects.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.Leader:          return this.Leaders.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.Object:          return this.Objects.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.ObjectType:      return this.ObjectTypes.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.Power:           return this.Powers.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.Squad:           return this.Squads.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.Tactic:          return this.Tactics.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.Tech:            return this.Techs.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.TerrainTileType: return this.TerrainTileTypes.TryGetNameWithUndefined(id);
			// TODO: Should just use the Objects DBI AFAICT
			case DatabaseObjectKind.Unit:       return this.TryGetNameUnit(id);
			case DatabaseObjectKind.UserClass:  return this.UserClasses.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.WeaponType: return this.WeaponTypes.TryGetNameWithUndefined(id);

			default: throw new KSoft.Debug.UnreachableException(kind.ToString());
			}
		}
		#endregion

		#region IProtoDataObjectDatabaseProvider members
		Engine.XmlFileInfo IProtoDataObjectDatabaseProvider.SourceFileReference { get { return null; } }

		Collections.IBTypeNames IProtoDataObjectDatabaseProvider.GetNamesInterface(int objectKind)
		{
			var kind = (DatabaseObjectKind)objectKind;
			return this.GetNamesInterface(kind);
		}

		Collections.IHasUndefinedProtoMemberInterface IProtoDataObjectDatabaseProvider.GetMembersInterface(int objectKind)
		{
			var kind = (DatabaseObjectKind)objectKind;
			return this.GetNamesInterface/*GetMembersInterface*/(kind);
		}
		#endregion

		XML.BTriggerScriptSerializer mTriggerSerializer;
		internal void InitializeTriggerScriptSerializer()
		{
			this.mTriggerSerializer = new XML.BTriggerScriptSerializer(this.Engine);
		}
		public BTriggerSystem LoadScript(string scriptName, BTriggerScriptType type = BTriggerScriptType.TriggerScript)
		{
			var ctxt = this.mTriggerSerializer.StreamTriggerScriptGetContext(FA.Read, type, scriptName);
			var task = Task<bool>.Factory.StartNew((state) => {
				var _ctxt = state as XML.BTriggerScriptSerializer.StreamTriggerScriptContext;
				return this.mTriggerSerializer.TryStreamData(_ctxt.FileInfo, FA.Read, this.mTriggerSerializer.StreamTriggerScript, _ctxt);
			}, ctxt);

			return task.Result ? ctxt.Script : null;
		}
		public bool LoadScenarioScripts(string scnrPath)
		{
			var ctxt = this.mTriggerSerializer.StreamTriggerScriptGetContext(FA.Read, BTriggerScriptType.Scenario, scnrPath);
			var task = Task<bool>.Factory.StartNew((state) => {
				var _ctxt = state as XML.BTriggerScriptSerializer.StreamTriggerScriptContext;
				return this.mTriggerSerializer.TryStreamData(_ctxt.FileInfo, FA.Read, this.mTriggerSerializer.LoadScenarioScripts, _ctxt);
			}, ctxt);

			return task.Result;
		}

		protected abstract XML.BDatabaseXmlSerializerBase NewXmlSerializer();
		private XML.BDatabaseXmlSerializerBase mXmlSerializer;

		public bool Preload()
		{
			var xs = this.mXmlSerializer = this.NewXmlSerializer();//using (var xs = NewXmlSerializer())
			{
				return xs.Preload();
			}
		}

		public bool Load()
		{
			Contract.Assert(this.mXmlSerializer != null);

			var xs = this.mXmlSerializer;//using (var xs = NewXmlSerializer())
			{
				return xs.Load();
			}
		}

		public bool LoadAllTactics()
		{
			Contract.Assert(this.mXmlSerializer != null);

			var xs = this.mXmlSerializer;//using (var xs = NewXmlSerializer())
			{
				return xs.LoadAllTactics();
			}
		}

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (var xs = this.NewXmlSerializer())
			{
				s.SetSerializerInterface(xs);
				xs.Serialize(s);
				s.SetSerializerInterface(null);
			}
		}
		#endregion
	};
}