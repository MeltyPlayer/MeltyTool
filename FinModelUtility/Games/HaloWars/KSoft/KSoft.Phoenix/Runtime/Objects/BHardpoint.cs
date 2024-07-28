
using BUnitOppID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	struct BHardpoint
		: IO.IEndianStreamSerializable
	{
		public const int kMaxCount = 0x14;

		public float YawRotationRate, PitchRotationRate;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.YawRotationRate);
			s.Stream(ref this.PitchRotationRate);
		}
		#endregion
	};

	public struct BHardpointState
		: IO.IEndianStreamSerializable
	{
		public int OwnerAction;
		public float AutoCenteringTimer, YawSoundActivationTimer, PitchSoundActivationTimer;
		public BUnitOppID OppID;
		public bool
			AllowAutoCentering, YawSound, YawSoundPlaying,
			PitchSound, PitchSoundPlaying, SecondaryTurretScanToken
			;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.OwnerAction);
			s.Stream(ref this.AutoCenteringTimer);	s.Stream(ref this.YawSoundActivationTimer);	s.Stream(ref this.PitchSoundActivationTimer);
			s.Stream(ref this.OppID);
			s.Stream(ref this.AllowAutoCentering);	s.Stream(ref this.YawSound);				s.Stream(ref this.YawSoundPlaying);
			s.Stream(ref this.PitchSound);			s.Stream(ref this.PitchSoundPlaying);	s.Stream(ref this.SecondaryTurretScanToken);
		}
		#endregion
	};
}