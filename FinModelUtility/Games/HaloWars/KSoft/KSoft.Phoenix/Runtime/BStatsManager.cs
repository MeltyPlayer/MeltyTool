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
			s.Stream(ref this.Lost);
			s.Stream(ref this.Destroyed);
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
			s.Stream(ref this.Key);
			s.Stream(ref this.Value);
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
			s.Stream(ref this.Index);
			BSaveGame.StreamArray16(s, ref this.Killers);
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
			BSaveGame.StreamArray16(s, ref this.Levels);
			s.Stream(ref this.XP);
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
			s.Stream(ref this.Index);
			s.Stream(this.Combat);
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
			s.Stream(this.KillerIDs);
			s.Stream(ref this.Built); s.Stream(ref this.Lost); s.Stream(ref this.Destroyed); s.Stream(ref this.Max);
			s.Stream(ref this.CombatID);
			s.Stream(ref this.FirstTime); s.Stream(ref this.LastTime);
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

			s.Stream(ref this.Timestamp);
			s.Stream(ref this.ProtoID);
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
			s.Stream(ref this.Key);
			s.Stream(this.Value);
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
			BSaveGame.StreamArray16(s, ref this.Killers, isIterated:true);
			BSaveGame.StreamArray16(s, ref this.Combat);
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

			BSaveGame.StreamCollection(s, this.Totals);
			s.Stream(this.Total);
			s.StreamNotNull(ref this.Combat_);
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

			s.Stream(this.Total);
			BSaveGame.StreamArray16(s, ref this.Events);
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
			s.Stream(ref this.Index);
			s.Stream(ref this.StatType);
			s.Stream(ref this.Stat, 
				() => FromType(this.StatType));
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
			s.Stream(ref this.Key);
			s.Stream(ref this.Value);
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
			s.Stream(ref this.Key);
			s.Stream(ref this.Value);
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

			BSaveGame.StreamArray16(s, ref this.Recorders, isIterated:true);
			s.StreamSignature(cSaveMarker.StatsRecorders);
			BSaveGame.StreamCollection(s, this.Powers);
			s.StreamSignature(cSaveMarker.StatsPowers);
			BSaveGame.StreamCollection(s, this.Abilities);
			s.StreamSignature(cSaveMarker.StatsAbilities);
			sg.StreamBCost(s, ref this.TotalResources); sg.StreamBCost(s, ref this.MaxResources);
			sg.StreamBCost(s, ref this.GatheredResources); sg.StreamBCost(s, ref this.TributedResources);
			s.Stream(ref this.PlayerID);
			s.Stream(ref this.TeamID);
			s.Stream(ref this.PlayerStateTime);
			s.Stream(ref this.PlayerState);
			s.Stream(ref this.StrengthTime); s.Stream(ref this.StrengthTimer);
			s.Stream(ref this.CivID); s.Stream(ref this.LeaderID);
			s.Stream(ref this.ResourcesUsed); s.Stream(ref this.PlayerType);
			s.Stream(ref this.RandomCiv); s.Stream(ref this.RandomLeader); s.Stream(ref this.Resigned);
			s.Stream(ref this.Defeated); s.Stream(ref this.Disconnected); s.Stream(ref this.Won);
			s.StreamSignature(cSaveMarker.StatsPlayer);
		}
		#endregion
	};
}
