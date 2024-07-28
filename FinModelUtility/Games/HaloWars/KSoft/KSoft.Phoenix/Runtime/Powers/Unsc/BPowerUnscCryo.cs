
using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;
using BEntityTimePair = System.UInt64;
using BProtoObjectID = System.Int32;
using BObjectTypeID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscCryo
		: BPower
	{
		public BEntityTimePair[] IgnoreList;
		public uint NextTickTime;
		public BEntityID CryoObjectID;
		public BVector Direction, Right;
		public BProtoObjectID CryoObjectProtoID, BomberProtoID;
		public BObjectTypeID FilterTypeID;
		public float CryoRadius, MinCryoFalloff;
		public uint TickDuration, TicksRemaining;
		public float CryoAmountPerTick, KillableHpLeft, FreezingThawTime, FrozenThawTime;
		public BPowerHelperBomber BomberData = new BPowerHelperBomber();
		public bool ReactionPlayed;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			BSaveGame.StreamArray(s, ref this.IgnoreList);
			s.Stream(ref this.NextTickTime);
			s.Stream(ref this.CryoObjectID);
			s.StreamV(ref this.Direction); s.StreamV(ref this.Right);
			s.Stream(ref this.CryoObjectProtoID); s.Stream(ref this.BomberProtoID);
			s.Stream(ref this.FilterTypeID);
			s.Stream(ref this.CryoRadius); s.Stream(ref this.MinCryoFalloff);
			s.Stream(ref this.TickDuration); s.Stream(ref this.TicksRemaining);
			s.Stream(ref this.CryoAmountPerTick); s.Stream(ref this.KillableHpLeft); s.Stream(ref this.FreezingThawTime); s.Stream(ref this.FrozenThawTime);
			s.Stream(this.BomberData);
			s.Stream(ref this.ReactionPlayed);
		}
		#endregion
	};
}