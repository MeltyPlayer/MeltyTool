
using BEntityID = System.Int32;
using BEntityTimePair = System.UInt64;
using BProtoObjectID = System.Int32;
using BObjectTypeID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscHeal
		: BPower
	{
		public BEntityID[] SquadsRepairing;
		public BEntityTimePair[] IgnoreList;
		public uint NextTickTime;
		public BEntityID RepairObjectID;
		public BProtoObjectID RepairAttachmentProtoID;
		public BObjectTypeID FilterTypeID;
		public float RepairRadius;
		public uint TickDuration;
		public float RepairCombatValuePerTick;
		public uint CooldownTimeIfDamaged, TicksRemaining;
		public bool SpreadAmongSquads, AllowReinforce, IgnorePlacement,
			HealAny, NeverStops;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			BSaveGame.StreamArray(s, ref this.SquadsRepairing);
			BSaveGame.StreamArray(s, ref this.IgnoreList);
			s.Stream(ref this.NextTickTime);
			s.Stream(ref this.RepairObjectID);
			s.Stream(ref this.RepairAttachmentProtoID);
			s.Stream(ref this.FilterTypeID);
			s.Stream(ref this.RepairRadius);
			s.Stream(ref this.TickDuration);
			s.Stream(ref this.RepairCombatValuePerTick);
			s.Stream(ref this.CooldownTimeIfDamaged); s.Stream(ref this.TicksRemaining);
			s.Stream(ref this.SpreadAmongSquads); s.Stream(ref this.AllowReinforce); s.Stream(ref this.IgnorePlacement);
			s.Stream(ref this.HealAny); s.Stream(ref this.NeverStops);
		}
		#endregion
	};
}