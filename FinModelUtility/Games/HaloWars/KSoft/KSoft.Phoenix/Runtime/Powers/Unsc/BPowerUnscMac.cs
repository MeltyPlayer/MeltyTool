
using BVector = System.Numerics.Vector4;
using BCueIndex = System.Int32;
using BEntityID = System.Int32;
using BProtoObjectID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscMac
		: BPower
	{
		public sealed class BShot
			: IO.IEndianStreamSerializable
		{
			public BVector LaunchPos, TargetPos;
			public uint LaunchTime, CreateLaserTime;
			public BEntityID LaserObj;
			public bool LaserCreated;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.StreamV(ref this.LaunchPos); s.StreamV(ref this.TargetPos);
				s.Stream(ref this.LaunchTime); s.Stream(ref this.CreateLaserTime);
				s.Stream(ref this.LaserObj);
				s.Stream(ref this.LaserCreated);
			}
			#endregion
		};

		public BEntityID RealTargettingLaserID;
		public BVector DesiredTargettingPosition;
		public BShot[] Shots;
		public bool FiredInitialShot;
		public uint ShotsRemaining, ImpactsToProcess;
		public BProtoObjectID TargetBeamID, ProjectileID, EffectProtoID,
			RockSmallProtoID, RockMediumProtoID, RockLargeProtoID;
		public BCueIndex FiredSound;
		public uint TargetingDelay, AutoShotDelay;
		public float AutoShotInnerRadius, AutoShotOuterRadius, 
			XOffset, YOffset, ZOffset;
		public int LOSMode;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(ref this.RealTargettingLaserID);
			s.StreamV(ref this.DesiredTargettingPosition);
			BSaveGame.StreamArray(s, ref this.Shots);
			s.Stream(ref this.FiredInitialShot);
			s.Stream(ref this.ShotsRemaining); s.Stream(ref this.ImpactsToProcess);
			s.Stream(ref this.TargetBeamID); s.Stream(ref this.ProjectileID); s.Stream(ref this.EffectProtoID);
			s.Stream(ref this.RockSmallProtoID); s.Stream(ref this.RockMediumProtoID); s.Stream(ref this.RockLargeProtoID);
			s.Stream(ref this.FiredSound);
			s.Stream(ref this.TargetingDelay); s.Stream(ref this.AutoShotDelay);
			s.Stream(ref this.AutoShotInnerRadius); s.Stream(ref this.AutoShotOuterRadius);
			s.Stream(ref this.XOffset); s.Stream(ref this.YOffset); s.Stream(ref this.ZOffset);
			s.Stream(ref this.LOSMode);
		}
		#endregion
	};
}