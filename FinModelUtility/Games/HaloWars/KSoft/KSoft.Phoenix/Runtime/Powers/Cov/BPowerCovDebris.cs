using System.Collections.Generic;

using BVector = System.Numerics.Vector4;
using BCost = System.Single;
using BCueIndex = System.Int32;
using BEntityID = System.Int32;
using BProtoObjectID = System.Int32;
using BTeamID = System.Int32;
using BWaveGravityBall = System.UInt64; // idk, 8 bytes
using BQueuedObject = System.UInt64; // idk, 8 bytes

namespace KSoft.Phoenix.Runtime
{
	class BPowerCovDebris : BPower
	{
		public double NextTickTime;
		public BWaveGravityBall RealGravityBall;
		public BVector DesiredBallPosition;
		public List<BEntityID> CapturedUnits = new List<BEntityID>();
		public float ExplodeCooldownLeft;
		public BEntityID[] UnitsToPull;
		public BQueuedObject[] QueuedPickupObjects;
		public BCost[] CostPerTick;
		public float TickLength;
		public BProtoObjectID BallProtoID, LightningProtoID, LightningBeamVisualProtoID,
			DebrisProtoID, ExplodeProtoID, PickupAttachmentProtoID;
		public float AudioReactionTimer;
		public uint LeaderAnimOrderID;
		public float MaxBallSpeedStagnant, MaxBallSpeedPulling, ExplodeTime,
			PullingRange, ExplosionForceOnDebris, HealthToCapture,
			NudgeStrength, InitialLateralPullStrength, CapturedRadialSpacing,
			CapturedSpringStrength, CapturedSpringDampening, CapturedSpringRestLength,
			CapturedMinLateralSpeed, RipAttachmentChancePulling, PickupObjectRate,
			DebrisAngularDamping, CurrentExplosionDamageBank, MaxPossibleExplosionDamageBank,
			MaxExplosionDamageBankPerCaptured, ExplosionDamageBankPerTick;
		public uint CommandInterval;
		public float MinBallDistance, MaxBallDistance;
		public int LightningPerTick, MaxCapturedObjects;
		public byte NudgeChancePulling, ThrowPartChancePulling, LightningChancePulling;
		public BCueIndex ExplodeSound;
		public float MinDamageBankPercentToThrow;
		public BTeamID[] RevealedTeamIDs;
		BProtoActionTacticUnion unknown0, unknown1, unknown2;
		public bool CompletedInitialization, ThrowUnitsOnExplosion;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			var sg = s.Owner as BSaveGame;

			s.Stream(ref this.NextTickTime);
			s.Stream(ref this.RealGravityBall);
			s.StreamV(ref this.DesiredBallPosition);
			BSaveGame.StreamCollection(s, this.CapturedUnits);
			s.Stream(ref this.ExplodeCooldownLeft);
			BSaveGame.StreamArray(s, ref this.UnitsToPull);
			BSaveGame.StreamArray(s, ref this.QueuedPickupObjects);
			sg.StreamBCost(s, ref this.CostPerTick);
			s.Stream(ref this.TickLength);
			s.Stream(ref this.BallProtoID); s.Stream(ref this.LightningProtoID); s.Stream(ref this.LightningBeamVisualProtoID);
			s.Stream(ref this.DebrisProtoID); s.Stream(ref this.ExplodeProtoID); s.Stream(ref this.PickupAttachmentProtoID);
			s.Stream(ref this.AudioReactionTimer);
			s.Stream(ref this.LeaderAnimOrderID);

			s.Stream(ref this.MaxBallSpeedStagnant); s.Stream(ref this.MaxBallSpeedPulling); s.Stream(ref this.ExplodeTime);
			s.Stream(ref this.PullingRange); s.Stream(ref this.ExplosionForceOnDebris); s.Stream(ref this.HealthToCapture);
			s.Stream(ref this.NudgeStrength); s.Stream(ref this.InitialLateralPullStrength); s.Stream(ref this.CapturedRadialSpacing);
			s.Stream(ref this.CapturedSpringStrength); s.Stream(ref this.CapturedSpringDampening); s.Stream(ref this.CapturedSpringRestLength);
			s.Stream(ref this.CapturedMinLateralSpeed); s.Stream(ref this.RipAttachmentChancePulling); s.Stream(ref this.PickupObjectRate);
			s.Stream(ref this.DebrisAngularDamping); s.Stream(ref this.CurrentExplosionDamageBank); s.Stream(ref this.MaxPossibleExplosionDamageBank);
			s.Stream(ref this.MaxExplosionDamageBankPerCaptured); s.Stream(ref this.ExplosionDamageBankPerTick);
			s.Stream(ref this.CommandInterval);
			s.Stream(ref this.MinBallDistance); s.Stream(ref this.MaxBallDistance);
			s.Stream(ref this.LightningPerTick); s.Stream(ref this.MaxCapturedObjects);
			s.Stream(ref this.NudgeChancePulling); s.Stream(ref this.ThrowPartChancePulling); s.Stream(ref this.LightningChancePulling);
			s.Stream(ref this.ExplodeSound);
			s.Stream(ref this.MinDamageBankPercentToThrow);
			BSaveGame.StreamArray(s, ref this.RevealedTeamIDs);
			s.Stream(ref this.unknown0); s.Stream(ref this.unknown1); s.Stream(ref this.unknown2);
			s.Stream(ref this.CompletedInitialization); s.Stream(ref this.ThrowUnitsOnExplosion);
		}
		#endregion
	};
}