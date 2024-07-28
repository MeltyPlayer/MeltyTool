
using BVector = System.Numerics.Vector4;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerCovRageUser
		: BPowerUser
	{
		public float CameraZoom, CameraHeight;
		public uint TimestampNextCommand;
		public BVector MoveInputDir, AttackInputDir;
		public float TimeUntilHint;
		public BVector LastMovePos, LastMoveDir, LastAttackDir;
		public uint CommandInterval;
		public float ScanRadius, MovementProjectionMultiplier;
		public bool HasMoved, HasAttacked, HintShown, 
			HintCompleted, ForceCommandNextUpdate;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(ref this.CameraZoom); s.Stream(ref this.CameraHeight);
			s.Stream(ref this.TimestampNextCommand);
			s.StreamV(ref this.MoveInputDir); s.StreamV(ref this.AttackInputDir);
			s.Stream(ref this.TimeUntilHint);
			s.StreamV(ref this.LastMovePos); s.StreamV(ref this.LastMoveDir); s.StreamV(ref this.LastAttackDir);
			s.Stream(ref this.CommandInterval);
			s.Stream(ref this.ScanRadius); s.Stream(ref this.MovementProjectionMultiplier);
			s.Stream(ref this.HasMoved); s.Stream(ref this.HasAttacked); s.Stream(ref this.HintShown);
			s.Stream(ref this.HintCompleted); s.Stream(ref this.ForceCommandNextUpdate);
		}
		#endregion
	};
}