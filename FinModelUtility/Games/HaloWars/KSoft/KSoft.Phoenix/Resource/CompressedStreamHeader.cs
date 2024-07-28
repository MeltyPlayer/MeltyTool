
namespace KSoft.Phoenix.Resource
{
	partial class CompressedStream
	{
		struct Header
			: IO.IEndianStreamSerializable
		{
			public const int kSizeOf = 0x24;

			internal uint HeaderAdler32;
			public uint StreamMode;
			public ulong UncompressedSize, CompressedSize;
			public uint UncompressedAdler32, CompressedAdler32;

			public bool UseBufferedStreaming { get {
				return this.StreamMode == (uint)Mode.Buffered;
			} }

			public void UpdateHeaderCrc()
			{
				this.HeaderAdler32 = InMemoryRep.Checksum(this.UncompressedSize,
				                                          this.UncompressedAdler32,
				                                          this.CompressedSize,
				                                          this.CompressedAdler32,
				                                          this.StreamMode);
			}

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				if (s.IsWriting)
					this.UpdateHeaderCrc();

				s.StreamSignature(kSignature);
				s.Stream(ref this.HeaderAdler32);
				s.Stream(ref this.StreamMode);
				s.Stream(ref this.UncompressedSize);
				s.Stream(ref this.CompressedSize);
				s.Stream(ref this.UncompressedAdler32);
				s.Stream(ref this.CompressedAdler32);
			}
			#endregion
		};
	};
}