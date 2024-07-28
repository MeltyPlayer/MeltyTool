
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscMacUser
		: BPowerUser
	{
		public BPowerHelperHudSounds HudSounds = new BPowerHelperHudSounds();
		public string HelpString;
		public BEntityID FakeTargettingLaserID, RealTargettingLaserID, TargettedSquadID;
		public uint ShotsRemaining;
		public float LastCommandSent, CommandInterval, LastShotSent, ShotInterval;
		public int LOSMode;
		public bool FlagLastCameraAutoZoomInstantEnabled, FlagLastCameraAutoZoomEnabled,
			FlagLastCameraZoomEnabled, FlagLastCameraYawEnabled;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(this.HudSounds);
			s.StreamPascalWideString32(ref this.HelpString);
			s.Stream(ref this.FakeTargettingLaserID); s.Stream(ref this.RealTargettingLaserID); s.Stream(ref this.TargettedSquadID);
			s.Stream(ref this.ShotsRemaining);
			s.Stream(ref this.LastCommandSent); s.Stream(ref this.CommandInterval); s.Stream(ref this.LastShotSent); s.Stream(ref this.ShotInterval);
			s.Stream(ref this.LOSMode);
			s.Stream(ref this.FlagLastCameraAutoZoomInstantEnabled); s.Stream(ref this.FlagLastCameraAutoZoomEnabled);
			s.Stream(ref this.FlagLastCameraZoomEnabled); s.Stream(ref this.FlagLastCameraYawEnabled);
		}
		#endregion
	};
}