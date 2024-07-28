
namespace KSoft.Phoenix.Runtime
{
	public class CondensedListInfo
	{
		public bool SerializeCapacity { get; set; }
		public int IndexSize { get; set; } = sizeof(short);
		public int MaxCount { get; set; } = ushort.MaxValue;
		public int DoneIndex { get; set; } = -1;

		public void StreamCapacity(IO.EndianStream s, ref int capacity)
		{
			if (!this.SerializeCapacity) return;

			switch (this.IndexSize)
			{
			case sizeof(byte):	byte cap8 = (byte)capacity;		s.Stream(ref cap8); capacity = cap8; break;
			case sizeof(ushort):ushort cap16 = (ushort)capacity;s.Stream(ref cap16);capacity = cap16; break;
			case sizeof(int):	s.Stream(ref capacity); break;

			default: throw new KSoft.Debug.UnreachableException(this.IndexSize.ToString());
			}
		}
		public void StreamDoneIndex(IO.EndianStream s)
		{
			switch (this.IndexSize)
			{
			case sizeof(byte):   s.StreamSignature((byte) this.DoneIndex); break;
			case sizeof(ushort): s.StreamSignature((ushort) this.DoneIndex); break;
			case sizeof(uint):   s.StreamSignature((uint) this.DoneIndex); break;

			default: throw new KSoft.Debug.UnreachableException(this.IndexSize.ToString());
			}
		}
	};
}
