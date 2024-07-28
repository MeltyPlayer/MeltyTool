using System.Collections.Generic;

using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;
using BProtoObjectID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscCarpetBomb
		: BPower
	{
		public sealed class BBombExplodeInfo
			: IO.IEndianStreamSerializable
		{
			public ulong Unknown0, Unknown8, Unknown10, Unknown18;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.Unknown0); s.Stream(ref this.Unknown8); s.Stream(ref this.Unknown10); s.Stream(ref this.Unknown18);
			}
			#endregion
		};

		public BBombExplodeInfo[] BombExplodeInfos;
		public List<BEntityID> NudgedUnits = new List<BEntityID>();
		public BVector StartLocation, StartDirection, RightVector;
		public sbyte State;
		public bool GotStartLocation, GotStartDirection;
		public double TickLength, NextBombTime, LastBombTime;
		public uint NumBombClustersDropped;
		public BProtoObjectID ProjectileProtoID, ImpactProtoID, ExplosionProtoID, BomberProtoID;
		public float InitialDelay, FuseTime;
		public uint MaxBombs;
		public float MaxBombOffset, BombSpacing, LengthMultiplier,
			WedgeLengthMultiplier, WedgeMinOffset, NudgeMultiplier;
		public BPowerHelperBomber BomberData = new BPowerHelperBomber();
		public sbyte LOSMode;
		public bool ReactionPlayed;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			BSaveGame.StreamArray(s, ref this.BombExplodeInfos);
			BSaveGame.StreamCollection(s, this.NudgedUnits);
			s.StreamV(ref this.StartLocation); s.StreamV(ref this.StartDirection); s.StreamV(ref this.RightVector);
			s.Stream(ref this.State);
			s.Stream(ref this.GotStartLocation); s.Stream(ref this.GotStartDirection);
			s.Stream(ref this.TickLength); s.Stream(ref this.NextBombTime); s.Stream(ref this.LastBombTime);
			s.Stream(ref this.NumBombClustersDropped);
			s.Stream(ref this.ProjectileProtoID); s.Stream(ref this.ImpactProtoID); s.Stream(ref this.ExplosionProtoID); s.Stream(ref this.BomberProtoID);
			s.Stream(ref this.InitialDelay); s.Stream(ref this.FuseTime);
			s.Stream(ref this.MaxBombs);
			s.Stream(ref this.MaxBombOffset); s.Stream(ref this.BombSpacing); s.Stream(ref this.LengthMultiplier);
			s.Stream(ref this.WedgeLengthMultiplier); s.Stream(ref this.WedgeMinOffset); s.Stream(ref this.NudgeMultiplier);
			s.Stream(this.BomberData);
			s.Stream(ref this.LOSMode);
			s.Stream(ref this.ReactionPlayed);
		}
		#endregion
	};
}