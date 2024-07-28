
using BVector = System.Numerics.Vector4;
using BCueIndex = System.Int32;
using BEntityID = System.Int32;
using BProtoObjectID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscDisruption
		: BPower
	{
		public BEntityID DisruptionObjectID;
		public float DisruptionRadius, DisruptionRadiusSqr, TimeRemainingSec,
			DisruptionStartTime;
		public BVector Direction, Right;
		public BProtoObjectID BomberProtoID, DisruptionObjectProtoID, 
			PulseObjectProtoID, StrikeObjectProtoID;
		public float PulseSpacing, NextPulseTime;
		public int NumPulses;
		public BPowerHelperBomber BomberData = new BPowerHelperBomber();
		public BCueIndex PulseSound;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(ref this.DisruptionObjectID);
			s.Stream(ref this.DisruptionRadius); s.Stream(ref this.DisruptionRadiusSqr); s.Stream(ref this.TimeRemainingSec);
			s.Stream(ref this.DisruptionStartTime);
			s.StreamV(ref this.Direction); s.StreamV(ref this.Right);
			s.Stream(ref this.BomberProtoID); s.Stream(ref this.DisruptionObjectProtoID);
			s.Stream(ref this.PulseObjectProtoID); s.Stream(ref this.StrikeObjectProtoID);
			s.Stream(ref this.PulseSpacing); s.Stream(ref this.NextPulseTime);
			s.Stream(ref this.NumPulses);
			s.Stream(this.BomberData);
			s.Stream(ref this.PulseSound);
		}
		#endregion
	};
}