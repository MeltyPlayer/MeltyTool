
using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerTransport
		: BPower
	{
		public BVector PickupLocation;
		public BEntityID[] SquadsToTransport;
		public bool GotPickupLocation;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.StreamV(ref this.PickupLocation);
			BSaveGame.StreamArray(s, ref this.SquadsToTransport);
			s.Stream(ref this.GotPickupLocation);
		}
		#endregion
	};
}