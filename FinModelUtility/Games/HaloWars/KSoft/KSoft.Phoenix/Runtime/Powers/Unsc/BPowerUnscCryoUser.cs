
namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscCryoUser
		: BPowerUser
	{
		public BPowerHelperHudSounds HudSounds = new BPowerHelperHudSounds();

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(this.HudSounds);
		}
		#endregion
	};
}