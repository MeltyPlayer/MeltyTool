
using BVector = System.Numerics.Vector4;
using BWaveGravityBall = System.UInt64; // idk, 8 bytes

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerCovDebrisUser
		: BPowerUser
	{
		public BWaveGravityBall FakeGravityBall;
		public BVector HorizontalMoveInputDir, VerticalMoveInputDir, 
			LastUpdatePos, CameraFocusPoint;
		public uint TimestampNextCommand;
		public float TimeUntilHint;
		public uint CommandInterval;
		public float MinBallDistance, MaxBallDistance, MinBallHeight,
			MaxBallHeight, MaxBallSpeedStagnant, MaxBallSpeedPulling,
			CameraDistance, CameraHeight, CameraHoverPointDistance,
			CameraMaxBallAngle, PullingRange, PickupShakeDuration,
			PickupRumbleShakeStrength, PickupCameraShakeStrength, 
			ExplodeTime, DelayShutdownTimeLeft;
		public bool HintShown, HintCompleted, ShuttingDown;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(ref this.FakeGravityBall);
			s.StreamV(ref this.HorizontalMoveInputDir); s.StreamV(ref this.VerticalMoveInputDir);
			s.StreamV(ref this.LastUpdatePos); s.StreamV(ref this.CameraFocusPoint);
			s.Stream(ref this.TimestampNextCommand);
			s.Stream(ref this.TimeUntilHint);
			s.Stream(ref this.CommandInterval);
			s.Stream(ref this.MinBallDistance); s.Stream(ref this.MaxBallDistance); s.Stream(ref this.MinBallHeight);
			s.Stream(ref this.MaxBallHeight); s.Stream(ref this.MaxBallSpeedStagnant); s.Stream(ref this.MaxBallSpeedPulling);
			s.Stream(ref this.CameraDistance); s.Stream(ref this.CameraHeight); s.Stream(ref this.CameraHoverPointDistance);
			s.Stream(ref this.CameraMaxBallAngle); s.Stream(ref this.PullingRange); s.Stream(ref this.PickupShakeDuration);
			s.Stream(ref this.PickupRumbleShakeStrength); s.Stream(ref this.PickupCameraShakeStrength);
			s.Stream(ref this.ExplodeTime); s.Stream(ref this.DelayShutdownTimeLeft);
			s.Stream(ref this.HintShown); s.Stream(ref this.HintCompleted); s.Stream(ref this.ShuttingDown);
		}
		#endregion
	};
}