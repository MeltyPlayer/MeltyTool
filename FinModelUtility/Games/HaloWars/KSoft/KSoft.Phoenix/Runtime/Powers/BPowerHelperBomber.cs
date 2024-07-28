
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerHelperBomber
		: IO.IEndianStreamSerializable
	{
		public BEntityID BomberId;
		public float BomberFlyinDistance, BomberFlyinHeight, BomberBombHeight,
			BomberSpeed, BombTime, FlyoutTime, AdditionalHeight;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.BomberId);
			s.Stream(ref this.BomberFlyinDistance); s.Stream(ref this.BomberFlyinHeight); s.Stream(ref this.BomberBombHeight);
			s.Stream(ref this.BomberSpeed); s.Stream(ref this.BombTime); s.Stream(ref this.FlyoutTime); s.Stream(ref this.AdditionalHeight);
		}
		#endregion
	};
}