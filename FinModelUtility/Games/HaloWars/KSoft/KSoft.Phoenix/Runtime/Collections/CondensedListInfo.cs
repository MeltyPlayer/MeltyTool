
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
			if (!SerializeCapacity) return;

			switch (IndexSize)
			{
			case sizeof(byte):	byte cap8 = (byte)capacity;		s.Stream(ref cap8); capacity = cap8; break;
			case sizeof(ushort):ushort cap16 = (ushort)capacity;s.Stream(ref cap16);capacity = cap16; break;
			case sizeof(int):	s.Stream(ref capacity); break;

			default: throw new KSoft.Debug.UnreachableException(IndexSize.ToString());
			}
		}
		public void StreamDoneIndex(IO.EndianStream s)
		{
			switch (IndexSize)
			{
			case sizeof(byte):s.StreamSignature((byte)DoneIndex); break;
			case sizeof(ushort): s.StreamSignature((ushort)DoneIndex); break;
			case sizeof(uint): s.StreamSignature((uint)DoneIndex); break;

			default: throw new KSoft.Debug.UnreachableException(IndexSize.ToString());
			}
		}
	};
}
