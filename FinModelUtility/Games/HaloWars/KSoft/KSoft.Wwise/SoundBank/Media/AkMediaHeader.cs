
namespace KSoft.Wwise.SoundBank
{
	public struct AkMediaHeader
		: IO.IEndianStreamSerializable
	{
		/// <summary>Size of this struct on disk</summary>
		public const int kSizeOf = sizeof(uint) * 3;

		public uint ID, Offset, Size;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.ID);
			s.Stream(ref this.Offset);
			s.Stream(ref this.Size);
		}
		#endregion
	};
}