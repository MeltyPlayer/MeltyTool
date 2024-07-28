
using BProtoPowerID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerEntry
		: IO.IEndianStreamSerializable
		//, IO.IIndentedTextWritable
	{
		public BPowerEntryItem[] Items;
		public BProtoPowerID ProtoPowerID;
		public int TimesUsed, IconLocation;
		public bool IgnoreCost, IgnoreTechPrereqs, IgnorePop;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray16(s, ref this.Items);
			s.Stream(ref this.ProtoPowerID);
			s.Stream(ref this.TimesUsed); s.Stream(ref this.IconLocation);
			s.Stream(ref this.IgnoreCost); s.Stream(ref this.IgnoreTechPrereqs); s.Stream(ref this.IgnorePop);
		}
		#endregion

		#region IIndentedStreamWritable Members
#if false
		public void ToStream(IO.IndentedTextWriter s)
		{
			var sg = s.Owner as BSaveGame;

			s.WriteLine("{1}\t{2}\t{3}\t{4}\t{5}\t{0}", sg.Database.ProtoPowers[ProtoPowerID],
				TimesUsed.ToString(), IconLocation.ToString(),
				IgnoreCost.ToString(), IgnoreTechPrereqs.ToString(), IgnorePop.ToString());
			using (s.EnterIndentBookmark())
				for (int x = 0; x < Items.Length; x++)
					Items[x].ToStream(s);
		}
#endif
		#endregion
	};
}