
using BCueIndex = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerHelperHudSounds
		: IO.IEndianStreamSerializable
	{
		public BCueIndex HudUpSound, HudAbortSound, HudFireSound,
			HudLastFireSound, HudStartEnvSound, HudStopEnvSound;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.HudUpSound); s.Stream(ref this.HudAbortSound); s.Stream(ref this.HudFireSound);
			s.Stream(ref this.HudLastFireSound); s.Stream(ref this.HudStartEnvSound); s.Stream(ref this.HudStopEnvSound);
		}
		#endregion
	};
}