
namespace KSoft.Phoenix.Runtime
{
	sealed class BTimerManager
		: IO.IEndianStreamSerializable
	{
		const int kTimerCount = 0x60 / BTimer.kSizeOf;

		public struct BTimer
			: IO.IEndianStreamSerializable
		{
			internal const int kSizeOf = 0x18; // in-memory rep size

			public uint StartTime, StopTime, CurrentTime, LastTime;
			public int ID;
			public bool CountUp, Active, Done, Paused;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.StartTime); s.Stream(ref this.StopTime); s.Stream(ref this.CurrentTime); s.Stream(ref this.LastTime);
				s.Stream(ref this.ID);
				s.Stream(ref this.CountUp); s.Stream(ref this.Active); s.Stream(ref this.Done); s.Stream(ref this.Paused);
			}
			#endregion
		};
		public BTimer[] Timers = new BTimer[kTimerCount];
		public int NextTimerID;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			for (int x = 0; x < this.Timers.Length; x++)
				s.Stream(ref this.Timers[x]);
			s.Stream(ref this.NextTimerID);
		}
		#endregion
	};
}