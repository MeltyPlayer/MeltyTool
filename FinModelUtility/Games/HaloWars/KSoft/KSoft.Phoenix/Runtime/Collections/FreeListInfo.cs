
namespace KSoft.Phoenix.Runtime
{
	public sealed class FreeListInfo
		: CondensedListInfo
	{
		public ushort SaveMarker
		{
			get { return (ushort) this.DoneIndex; }
			set { this.DoneIndex = value; }
		}

		public FreeListInfo(ushort saveMarker)
		{
			this.SerializeCapacity = true;
			this.IndexSize = sizeof(short);
			this.MaxCount = ushort.MaxValue;
			this.SaveMarker = saveMarker;
		}

		public void StreamCount(IO.EndianStream s, ref int count)
		{
			this.StreamCapacity(s, ref count);
		}
		public void StreamSaveMarker(IO.EndianStream s)
		{
			s.StreamSignature(this.SaveMarker);
		}
	};
}