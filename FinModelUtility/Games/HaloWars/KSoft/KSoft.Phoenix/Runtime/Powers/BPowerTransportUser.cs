
using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerTransportUser
		: BPowerUser
	{
		public BPowerHelperHudSounds HudSounds = new BPowerHelperHudSounds();
		public BEntityID[] SquadsToTransport;
		public BEntityID[] TargetedSquads;
		public int LOSMode;
		public bool GotPickupLocation;
		public BVector PickupLocation;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(this.HudSounds);
			BSaveGame.StreamArray(s, ref this.SquadsToTransport);
			BSaveGame.StreamArray(s, ref this.TargetedSquads);
			s.Stream(ref this.LOSMode);
			s.Stream(ref this.GotPickupLocation);
			s.StreamV(ref this.PickupLocation);
		}
		#endregion
	};
}