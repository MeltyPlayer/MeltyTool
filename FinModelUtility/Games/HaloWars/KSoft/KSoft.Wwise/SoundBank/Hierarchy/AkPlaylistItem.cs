
namespace KSoft.Wwise.SoundBank
{
	public struct AkPlaylistItem
		: IO.IEndianStreamSerializable
	{
		/// <summary>Size of this struct on disk</summary>
		internal const uint kSizeOf = 5;

		public uint ID;
		public sbyte Weight;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.ID);
			s.Stream(ref this.Weight);
		}
		#endregion
	};
}