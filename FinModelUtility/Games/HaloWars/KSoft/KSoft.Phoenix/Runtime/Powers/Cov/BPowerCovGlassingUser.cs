
using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerCovGlassingUser
		: BPowerUser
	{
		public BVector InputDir, LastUpdatePos;
		public BEntityID RealBeamID, FakeBeamID, AirImpactObjectID;
		public uint TimestampNextCommand, CommandInterval;
		public float MinBeamDistance, MaxBeamDistance, MaxBeamSpeed;
		public int LOSMode;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.StreamV(ref this.InputDir); s.StreamV(ref this.LastUpdatePos);
			s.Stream(ref this.RealBeamID); s.Stream(ref this.FakeBeamID); s.Stream(ref this.AirImpactObjectID);
			s.Stream(ref this.TimestampNextCommand); s.Stream(ref this.CommandInterval);
			s.Stream(ref this.MinBeamDistance); s.Stream(ref this.MaxBeamDistance); s.Stream(ref this.MaxBeamSpeed);
			s.Stream(ref this.LOSMode);
		}
		#endregion
	};
}