using System.Collections.Generic;

using BProtoPowerID = System.Int32;
using BPlayerID = System.Int32;
using BTeamID = System.Int32;
using BPlayerState = System.UInt32; // states are defined in GameData.xml
using BCost = System.Single;

namespace KSoft.Phoenix.Runtime
{
	partial class cSaveMarker
	{
		public const ushort
			StatsPlayer = 0x2710,
			StatsRecorders = 0x2711,
			StatsPowers = 0x2712,
			StatsAbilities = 0x2713
			;
	};

	struct BStatLostDestroyed
		: IO.IEndianStreamSerializable
	{
		public uint Lost, Destroyed;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref Lost);
			s.Stream(ref Destroyed);
		}
		#endregion
	};
	struct BStatLostDestroyedKeyValuePair
		: IO.IEndianStreamSerializable
	{
		public int Key;
		public BStatLostDestroyed Value;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref Key);
			s.Stream(ref Value);
		}
		#endregion
	};
	sealed class BStatLostDestroyedMap
		: IO.IEndianStreamSerializable
	{
		public int Index;
		public BStatLostDestroyedKeyValuePair[] Killers;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref Index);
			BSaveGame.StreamArray16(s, ref Killers);
		}
		#endregion
	};

	sealed class BStatCombat
		: IO.IEndianStreamSerializable
	{
		public const ushort DoneIndex = 0x2711;

		public int[] Levels;
		public float XP;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray16(s, ref Levels);
			s.Stream(ref XP);
		}
		#endregion
	};
	sealed class BStatCombatWithIndex
		: IO.IEndianStreamSerializable
	{
		public int Index;
		public BStatCombat Combat = new BStatCombat();

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref Index);
			s.Stream(Combat);
		}
		#endregion
	};

	class BStatTotal
		: IO.IEndianStreamSerializable
	{
		public byte[] KillerIDs = new byte[0x20];
		public uint Built, Lost, Destroyed, Max;
		public int CombatID;
		public uint FirstTime, LastTime;

		#region IEndianStreamSerializable Members
		public virtual void Serialize(IO.EndianStream s)
		{
			s.Stream(KillerIDs);
			s.Stream(ref Built); s.Stream(ref Lost); s.Stream(ref Destroyed); s.Stream(ref Max);
			s.Stream(ref CombatID);
			s.Stream(ref FirstTime); s.Stream(ref LastTime);
		}
		#endregion
	};
	sealed class BStatEvent
		: BStatTotal
	{
		public uint Timestamp;
		public int ProtoID;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(ref Timestamp);
			s.Stream(ref ProtoID);
		}
		#endregion
	};

	sealed class BStatTotalKeyValuePair
		: IO.IEndianStreamSerializable
	{
		public int Key;
		public BStatTotal Value = new BStatTotal();

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref Key);
			s.Stream(Value);
		}
		#endregion
	};

	#region BStats Recorders
	abstract class BStatRecorderBase
		: IO.IEndianStreamSerializable
	{
		public BStatLostDestroyedMap[] Killers;
		public BStatCombatWithIndex[] Combat;

		#region IEndianStreamSerializable Members
		public virtual void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray16(s, ref Killers, isIterated:true);
			BSaveGame.StreamArray16(s, ref Combat);
			s.StreamSignature(BStatCombat.DoneIndex);
		}
		#endregion
	};
	sealed class BStatTotalsRecorder
		: BStatRecorderBase
	{
		public const int kStatType = 1;

		public List<BStatTotalKeyValuePair> Totals { get; private set; } = new List<BStatTotalKeyValuePair>();
		public BStatTotal Total = new BStatTotal();
		public BStatCombat Combat_;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			BSaveGame.StreamCollection(s, Totals);
			s.Stream(Total);
			s.StreamNotNull(ref Combat_);
		}
		#endregion
	};
	sealed class BStatEventRecorder
		: BStatRecorderBase
	{
		public const int kStatType = 2;

		public BStatTotal Total = new BStatTotal();
		public BStatEvent[] Events;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(Total);
			BSaveGame.StreamArray16(s, ref Events);
		}
		#endregion
	};
	sealed class BStatGraphRecorder
		: BStatRecorderBase
	{
		public const int kStatType = 3;
	};
	sealed class BStatResourceGraphRecorder
		: BStatRecorderBase
	{
		public const int kStatType = 4;
	};
	sealed class BStatPopGraphRecorder
		: BStatRecorderBase
	{
		public const int kStatType = 5;
	};
	sealed class BStatBaseGraphRecorder
		: BStatRecorderBase
	{
		public const int kStatType = 6;
	};
	sealed class BStatScoreGraphRecorder
		: BStatRecorderBase
	{
		public const int kStatType = 7;
	};

	sealed class BStatsRecorder
		: IO.IEndianStreamSerializable
	{
		public short Index;

		public byte StatType;
		public BStatRecorderBase Stat;

		#region IEndianStreamSerializable Members
		static BStatRecorderBase FromType(int statType)
		{
			switch (statType)
			{
			case BStatTotalsRecorder.kStatType: return new BStatTotalsRecorder();
			case BStatEventRecorder.kStatType: return new BStatEventRecorder();
#if false
			case BStatGraphRecorder.kStatType: return new BStatGraphRecorder();
			case BStatResourceGraphRecorder.kStatType: return new BStatResourceGraphRecorder();
			case BStatPopGraphRecorder.kStatType: return new BStatPopGraphRecorder();
			case BStatBaseGraphRecorder.kStatType: return new BStatBaseGraphRecorder();
			case BStatScoreGraphRecorder.kStatType: return new BStatScoreGraphRecorder();
#endif

			default: throw new KSoft.Debug.UnreachableException(statType.ToString());
			}
		}
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref Index);
			s.Stream(ref StatType);
			s.Stream(ref Stat, 
				() => FromType(StatType));
		}
		#endregion
	};
	#endregion

	struct BStatPowerKeyValuePair
		: IO.IEndianStreamSerializable
	{
		public BProtoPowerID Key;
		public uint Value;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref Key);
			s.Stream(ref Value);
		}
		#endregion
	};
	struct BStatAbilityKeyValuePair
		: IO.IEndianStreamSerializable
	{
		public int Key;
		public uint Value;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref Key);
			s.Stream(ref Value);
		}
		#endregion
	};
	sealed class BStatsManager
		: IO.IEndianStreamSerializable
	{
		public BStatsRecorder[] Recorders;
		public List<BStatPowerKeyValuePair> Powers { get; private set; } = new List<BStatPowerKeyValuePair>();
		public List<BStatAbilityKeyValuePair> Abilities { get; private set; } = new List<BStatAbilityKeyValuePair>();

		public BCost[] TotalResources, MaxResources, 
			GatheredResources, TributedResources;
		public BPlayerID PlayerID;
		public BTeamID TeamID;
		public uint PlayerStateTime;
		public BPlayerState PlayerState;
		public uint StrengthTime, StrengthTimer;
		public int CivID, LeaderID;
		public byte ResourcesUsed, PlayerType;
		public bool RandomCiv, RandomLeader, Resigned, Defeated, Disconnected, Won;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			var sg = s.Owner as BSaveGame;

			BSaveGame.StreamArray16(s, ref Recorders, isIterated:true);
			s.StreamSignature(cSaveMarker.StatsRecorders);
			BSaveGame.StreamCollection(s, Powers);
			s.StreamSignature(cSaveMarker.StatsPowers);
			BSaveGame.StreamCollection(s, Abilities);
			s.StreamSignature(cSaveMarker.StatsAbilities);
			sg.StreamBCost(s, ref TotalResources); sg.StreamBCost(s, ref MaxResources);
			sg.StreamBCost(s, ref GatheredResources); sg.StreamBCost(s, ref TributedResources);
			s.Stream(ref PlayerID);
			s.Stream(ref TeamID);
			s.Stream(ref PlayerStateTime);
			s.Stream(ref PlayerState);
			s.Stream(ref StrengthTime); s.Stream(ref StrengthTimer);
			s.Stream(ref CivID); s.Stream(ref LeaderID);
			s.Stream(ref ResourcesUsed); s.Stream(ref PlayerType);
			s.Stream(ref RandomCiv); s.Stream(ref RandomLeader); s.Stream(ref Resigned);
			s.Stream(ref Defeated); s.Stream(ref Disconnected); s.Stream(ref Won);
			s.StreamSignature(cSaveMarker.StatsPlayer);
		}
		#endregion
	};
}
