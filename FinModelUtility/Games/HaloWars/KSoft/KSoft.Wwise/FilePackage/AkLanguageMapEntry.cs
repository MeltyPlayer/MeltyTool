
namespace KSoft.Wwise.FilePackage
{
	public struct AkLanguageMapEntry
		: IO.IEndianStreamSerializable
	{
		/// <summary>Size of this struct on disk</summary>
		public const int kSizeOf = sizeof(uint) * 2;

		public uint Offset;
		public uint ID;

		public string Value;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.Offset);
			s.Stream(ref this.ID);
		}
		#endregion
	};
}