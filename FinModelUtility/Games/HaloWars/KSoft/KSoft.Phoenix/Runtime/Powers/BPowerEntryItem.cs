
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerEntryItem
		: IO.IEndianStreamSerializable
		//, IO.IIndentedTextWritable
	{
		public BEntityID SquadID;
		public int UsesRemaining, TimesUsed, ChargeCap;
		public uint NextGrantTime;
		public bool InfiniteUses, Recharging;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.SquadID);
			s.Stream(ref this.UsesRemaining); s.Stream(ref this.TimesUsed); s.Stream(ref this.ChargeCap);
			s.Stream(ref this.NextGrantTime);
			s.Stream(ref this.InfiniteUses); s.Stream(ref this.Recharging);
		}
		#endregion

		#region IIndentedStreamWritable Members
#if false
		public void ToStream(IO.IndentedTextWriter s)
		{
			s.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
				UsesRemaining.ToString(), TimesUsed.ToString(), ChargeCap.ToString(),
				NextGrantTime.ToString(), InfiniteUses.ToString(), Recharging.ToString(),
				SquadID.ToString("X8"));
		}
#endif
		#endregion
	};
}