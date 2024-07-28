using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;
using BPlayerID = System.Int32;
using BTeamID = System.Int32;
using BPlayerState = System.UInt32; // states are defined in GameData.xml
using BCost = System.Single;
using BPowerLevel = System.UInt32; // idk, 4 bytes

namespace KSoft.Phoenix.Runtime
{
	using BRallyPoint = System.Numerics.Vector4; // this is only a guess

	partial class cSaveMarker
	{
		public const ushort
			Player1 = 0x2710, Player2 = 0x2711, Player3 = 0x2712, Player4 = 0x2713
			;
	};

	sealed class BPlayer
		: IO.IEndianStreamSerializable
	{
		public struct PowerInfo : IO.IEndianStreamSerializable
		{
			public uint RechargeTime;
			public int UseLimit;
			public BPowerLevel Level;
			public uint AvailableTime;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.RechargeTime);
				s.Stream(ref this.UseLimit);
				s.Stream(ref this.Level);
				s.Stream(ref this.AvailableTime);
			}
			#endregion
		};
		public struct RateInfo
			: IO.IEndianStreamSerializable
		{
			public float Amount, Multiplier;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.Amount);
				s.Stream(ref this.Multiplier);
			}
			#endregion
		};
		#region UnitCountInfo
		public struct UnitCountInfo
			: IO.IEndianStreamSerializable
		{
			public uint FutureUnitCount, DeadUnitCount;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.FutureUnitCount);
				s.Stream(ref this.DeadUnitCount);
			}
			#endregion
		};
		static readonly CondensedListInfo kUnitCountsListInfo = new CondensedListInfo()
		{
			IndexSize=sizeof(short),
		};
		#endregion
		#region WeaponType
		public struct WeaponType
			: IO.IEndianStreamSerializable
		{
			public sbyte DamageType;
			public float Modifier;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.DamageType);
				s.Stream(ref this.Modifier);
			}
			#endregion
		};
		// Index = actual WeaponType
		static readonly CondensedListInfo kWeaponTypesListInfo = new CondensedListInfo()
		{
			IndexSize=sizeof(sbyte),
			MaxCount=0x4E20,
		};
		#endregion

		#region Player1
		static readonly CondensedListInfo kProtoUnitsListInfo = new CondensedListInfo()
		{
			SerializeCapacity=true,
			IndexSize=sizeof(short),
			MaxCount=0x4E20,
		};
		static readonly CondensedListInfo kProtoTechsListInfo = new CondensedListInfo()
		{
			SerializeCapacity=true,
			IndexSize=sizeof(short),
			MaxCount=0x1388,
		};
		static readonly CondensedListInfo kProtoUniqueUnitsListInfo = new CondensedListInfo()
		{
			SerializeCapacity=true,
			IndexSize=sizeof(short),
			MaxCount=0x3E8,
		};

		public BVector LookAtPos; // Gaia's values are ints, not floats...
		public string Name;
		public BRallyPoint RallyPoint; // Gaia's values are ints, not floats...
		public BStatsManager StatsManager = new BStatsManager();
		public List<CondensedListItem16<BProtoObject>> ProtoObjects;
		public List<CondensedListItem16<BProtoSquad>> ProtoSquads;
		public List<CondensedListItem16<BProtoTech>> ProtoTechs;
		public BProtoObject[] UniqueProtoObjects;
		public BProtoSquad[] UniqueProtoSquad;
		public BPowerEntry[] PowerEntries;
		public int[] Abilities;
		public PowerInfo[] Powers;
		public BCost[] Resources;
		public RateInfo[] Rates;
		public BCost[] TotalResources;
		public BCost[] ResourceTrickleRate;
		public BPlayerPop[] Populations;
		#endregion
		#region Player2
		public BHintEngine HintEngine;
		public List<CondensedListItemValue16<UnitCountInfo>> GenericObjectCounts, SquadCounts;
		#endregion
		#region Player3
		public uint TotalFutureUnitCounts, TotalDeadUnitCounts;
		public uint TotalFutureSquadCounts, TotalDeadSquadCounts;
		public BEntityID[] GotoBases;
		public List<CondensedListItemValue8<WeaponType>> WeaponTypes;
		public float[] AbilityRecoverTimes;
		public BTechTree TechTree = new BTechTree();
		#endregion
		#region Player4
		public int MPID, ColorIndex;
		public BPlayerID ID, CoopID, ScenarioID;
		public int CivID;
		public BTeamID TeamID;
		public BPlayerState PlayerState;
		public int LeaderID, BountyResource;
		public BEntityID RallyObject;
		public int Strength;
		public float TributeCost;
		public BCost[] RepairCost;
		public float RepairTime, HandicapMultiplier, ShieldRegenRate;
		public uint ShieldRegenDelay;
		public float TotalCombatValue;
		public float Difficulty;
		public uint GamePlayedTime;
		public BPlayerID FloodPoofPlayer;
		public sbyte PlayerType, SquadSearchAttempts;
		public float WeaponPhysicsMultiplier, AIDamageMultiplier, AIDamageTakenMultiplier,
			AIBuildSpeedMultiplier;
		public bool FlagRallyPoint, FlagBountyResource, FlagMinimapBlocked,
			FlagLeaderPowersBlocked, FlagDefeatedDestroy;
		public int SquadAISearchIndex, SquadAIWorkIndex, SquadAISecondaryTurretScanIndex;
		#endregion

		#region IEndianStreamSerializable Members
		long mPositionMarker;
		public void Serialize(IO.EndianStream s)
		{
			var sg = s.Owner as BSaveGame;

			#region Init
			if (s.IsReading)
			{
				this.ProtoObjects = new List<CondensedListItem16<BProtoObject>>(sg.Database.GenericProtoObjects.Count);
				this.ProtoSquads = new List<CondensedListItem16<BProtoSquad>>(sg.Database.ProtoSquads.Count);
				this.ProtoTechs = new List<CondensedListItem16<BProtoTech>>(sg.Database.ProtoTechs.Count);

				this.Powers = new PowerInfo[sg.Database.ProtoPowers.Count];
				this.Rates = new RateInfo[sg.Database.Rates.Count];
				this.Populations = new BPlayerPop[sg.Database.Populations.Count];

				this.GenericObjectCounts = new List<CondensedListItemValue16<UnitCountInfo>>(sg.Database.GenericProtoObjects.Count);
				this.SquadCounts = new List<CondensedListItemValue16<UnitCountInfo>>(sg.Database.ProtoSquads.Count);
				this.WeaponTypes = new List<CondensedListItemValue8<WeaponType>>(sg.Database.WeaponTypes.Count);
			}
			#endregion

			#region Player1
			s.StreamV(ref this.LookAtPos);
			s.StreamPascalString32(ref this.Name);
			s.StreamV(ref this.RallyPoint);
			s.Stream(this.StatsManager);
			BSaveGame.StreamList(s, this.ProtoObjects, kProtoUnitsListInfo);
			BSaveGame.StreamList(s, this.ProtoSquads, kProtoUnitsListInfo);
			BSaveGame.StreamList(s, this.ProtoTechs, kProtoTechsListInfo);
			BSaveGame.StreamArray16(s, ref this.UniqueProtoObjects);
			Contract.Assert(this.UniqueProtoObjects.Length <= kProtoUniqueUnitsListInfo.MaxCount);
			BSaveGame.StreamArray16(s, ref this.UniqueProtoSquad);
			Contract.Assert(this.UniqueProtoSquad.Length <= kProtoUniqueUnitsListInfo.MaxCount);
			BSaveGame.StreamArray(s, ref this.PowerEntries);
			BSaveGame.StreamArray(s, ref this.Abilities);
			for (int x = 0; x < this.Powers.Length; x++)
				s.Stream(ref this.Powers[x]);
			sg.StreamBCost(s, ref this.Resources);
			for (int x = 0; x < this.Rates.Length; x++)
				s.Stream(ref this.Rates[x]);
			sg.StreamBCost(s, ref this.TotalResources);
			sg.StreamBCost(s, ref this.ResourceTrickleRate);
			for (int x = 0; x < this.Populations.Length; x++)
				s.Stream(ref this.Populations[x]);
			s.StreamSignature(cSaveMarker.Player1);
			#endregion
			#region Player2
			s.StreamNotNull(ref this.HintEngine);
			BSaveGame.StreamList(s, this.GenericObjectCounts, kUnitCountsListInfo);
			BSaveGame.StreamList(s, this.SquadCounts, kUnitCountsListInfo);
			s.StreamSignature(cSaveMarker.Player2);
			#endregion
			#region Player3
			s.Stream(ref this.TotalFutureUnitCounts); s.Stream(ref this.TotalDeadUnitCounts);
			s.Stream(ref this.TotalFutureSquadCounts); s.Stream(ref this.TotalDeadSquadCounts);
			BSaveGame.StreamArray(s, ref this.GotoBases);
			BSaveGame.StreamList(s, this.WeaponTypes, kWeaponTypesListInfo);
			BSaveGame.StreamArray16(s, ref this.AbilityRecoverTimes);
			s.Stream(this.TechTree);
			s.StreamSignature(cSaveMarker.Player3);
			#endregion
			#region Player4
			s.Stream(ref this.MPID); s.Stream(ref this.ColorIndex);
			s.Stream(ref this.ID); s.Stream(ref this.CoopID); s.Stream(ref this.ScenarioID);
			s.Stream(ref this.CivID);
			s.Stream(ref this.TeamID);
			s.Stream(ref this.PlayerState);
			s.Stream(ref this.LeaderID); s.Stream(ref this.BountyResource);
			s.Stream(ref this.RallyObject);
			s.Stream(ref this.Strength);
			s.Stream(ref this.TributeCost);
			sg.StreamBCost(s, ref this.RepairCost);
			s.Stream(ref this.RepairTime); s.Stream(ref this.HandicapMultiplier); s.Stream(ref this.ShieldRegenRate);
			s.Stream(ref this.ShieldRegenDelay);
			s.Stream(ref this.TotalCombatValue);
			s.Stream(ref this.Difficulty);
			s.Stream(ref this.GamePlayedTime);
			s.Stream(ref this.FloodPoofPlayer);
			s.Stream(ref this.PlayerType); s.Stream(ref this.SquadSearchAttempts);
			s.Stream(ref this.WeaponPhysicsMultiplier); s.Stream(ref this.AIDamageMultiplier); s.Stream(ref this.AIDamageTakenMultiplier);
			s.Stream(ref this.AIBuildSpeedMultiplier);
			s.Stream(ref this.FlagRallyPoint); s.Stream(ref this.FlagBountyResource); s.Stream(ref this.FlagMinimapBlocked);
			s.Stream(ref this.FlagLeaderPowersBlocked); s.Stream(ref this.FlagDefeatedDestroy);
			s.Stream(ref this.SquadAISearchIndex); s.Stream(ref this.SquadAIWorkIndex); s.Stream(ref this.SquadAISecondaryTurretScanIndex);
			s.StreamSignature(cSaveMarker.Player4);
			#endregion
			s.TraceAndDebugPosition(ref this.mPositionMarker);
		}
		#endregion
	};
}