
namespace KSoft.Phoenix.Runtime
{
	struct BPlayerPop
		: IO.IEndianStreamSerializable
	{
		public float Existing, Cap, Max, Future;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.Existing); s.Stream(ref this.Cap); s.Stream(ref this.Max); s.Stream(ref this.Future);
		}
		#endregion
#if false
		public void ToStream(IO.IndentedTextWriter s, string popName)
		{
			s.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}",
				Existing.ToString("r"), Cap.ToString("r"), Max.ToString("r"), Future.ToString("r"),
				popName);
		}
#endif
	};
}