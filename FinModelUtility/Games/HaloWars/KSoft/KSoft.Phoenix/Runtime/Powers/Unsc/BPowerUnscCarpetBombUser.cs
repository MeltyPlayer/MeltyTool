
using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;
using BProtoObjectID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscCarpetBombUser
		: BPowerUser
	{
		public sbyte InputState;
		public BVector Position, DesiredForward;
		public float DesiredScale, CurrentScale;
		public double ShutdownTime;
		public BEntityID ArrowID;
		public float MaxBombOffset, LengthMultiplier;
		public sbyte LOSMode;
		public BProtoObjectID ArrowProtoID;
		public BPowerHelperHudSounds HudSounds = new BPowerHelperHudSounds();

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(ref this.InputState);
			s.StreamV(ref this.Position); s.StreamV(ref this.DesiredForward);
			s.Stream(ref this.DesiredScale); s.Stream(ref this.CurrentScale);
			s.Stream(ref this.ShutdownTime);
			s.Stream(ref this.ArrowID);
			s.Stream(ref this.MaxBombOffset); s.Stream(ref this.LengthMultiplier);
			s.Stream(ref this.LOSMode);
			s.Stream(ref this.ArrowProtoID);
			s.Stream(this.HudSounds);
		}
		#endregion
	};
}