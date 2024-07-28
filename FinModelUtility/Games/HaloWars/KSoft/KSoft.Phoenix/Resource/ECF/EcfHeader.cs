using System;
using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

// http://en.wikipedia.org/wiki/Unix_File_System

namespace KSoft.Phoenix.Resource.ECF
{
	public struct EcfHeader
		: IO.IEndianStreamSerializable
	{
		public const uint kSignature = 0xDABA7737;
		public const int kSizeOf = 0x20;
		public const int kAdler32StartOffset = sizeof(uint) * 3;

		public int HeaderSize;
		// Checksum of the TotalSize field and onward, added to the checksum of everything after the header (HeaderSize - sizeof(ECFHeader))
		public uint Adler32;

		public int TotalSize;
		public short ChunkCount;
		private ushort mFlags;
		private uint mID; // The signature of the data which this header encapsulates
		private ushort mExtraDataSize;

		public int Adler32BufferLength { get { return this.HeaderSize - kAdler32StartOffset; } }
		public uint Id { get { return this.mID; } }
		public ushort ExtraDataSize { get { return this.mExtraDataSize; } }

		public void InitializeChunkInfo(uint dataId, uint dataChunkExtraDataSize = 0)
		{
			this.mID = dataId;
			this.mExtraDataSize = (ushort)dataChunkExtraDataSize;
		}

		public void BeginBlock(IO.IKSoftBinaryStream s)
		{
			s.VirtualAddressTranslationInitialize(Shell.ProcessorSize.x32);
			s.VirtualAddressTranslationPush(s.PositionPtr);
		}
		public void EndBlock(IO.IKSoftBinaryStream s)
		{
			s.VirtualAddressTranslationPop();
		}

		public void UpdateTotalSize(Stream s, int startOffset = 0)
		{
			Contract.Requires(startOffset >= 0);

			this.TotalSize = (int)(s.Length - startOffset);
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamSignature(kSignature);
			s.Stream(ref this.HeaderSize);
			s.Stream(ref this.Adler32);
			s.Stream(ref this.TotalSize);
			s.Stream(ref this.ChunkCount);
			s.Stream(ref this.mFlags);

			if (s.IsReading && this.mFlags != 0)
			{
				// TODO: trace
				System.Diagnostics.Debugger.Break();
			}

			s.Stream(ref this.mID);
			s.Stream(ref this.mExtraDataSize);
			s.Pad(sizeof(short) + sizeof(int));
		}
		#endregion

		public int CalculateChunkEntriesSize(
			int assumedChunkCount = TypeExtensions.kNone)
		{
			if (assumedChunkCount.IsNone())
				assumedChunkCount = this.ChunkCount;

			int entries_size = EcfChunk.kSizeOf;
			entries_size += this.ExtraDataSize;
			entries_size *= assumedChunkCount;

			return entries_size;
		}

		public uint ComputeAdler32(Stream stream, long headerPosition)
		{
			Contract.Requires(stream != null);
			Contract.Requires(headerPosition >= 0);

			long current_position = stream.Position;

			long adler_start_position = headerPosition + kAdler32StartOffset;
			stream.Seek(adler_start_position, SeekOrigin.Begin);
			var adler = Security.Cryptography.Adler32.Compute(stream, this.Adler32BufferLength);

			stream.Seek(current_position, SeekOrigin.Begin);

			return adler;
		}

		public void ComputeAdler32AndWrite(IO.EndianStream s, long headerPosition)
		{
			Contract.Requires(s != null);
			Contract.Requires(headerPosition >= 0);

			long current_position = s.BaseStream.Position;

			long adler_start_position = headerPosition + kAdler32StartOffset;
			s.BaseStream.Seek(adler_start_position, SeekOrigin.Begin);
			var adler = Security.Cryptography.Adler32.Compute(s.BaseStream, this.Adler32BufferLength);
			this.Adler32 = adler;

			s.BaseStream.Seek(headerPosition, SeekOrigin.Begin);
			this.Serialize(s);

			s.BaseStream.Seek(current_position, SeekOrigin.Begin);
		}

		public static bool VerifyIsEcf(IO.EndianReader s)
		{
			const int k_sizeof_signature = sizeof(uint);

			Contract.Requires<InvalidOperationException>(s.BaseStream.CanRead);
			Contract.Requires<InvalidOperationException>(s.BaseStream.CanSeek);

			var base_stream = s.BaseStream;
			if ((base_stream.Length - base_stream.Position) < k_sizeof_signature)
			{
				return false;
			}

			uint sig = s.ReadUInt32();
			base_stream.Seek(-k_sizeof_signature, SeekOrigin.Current);

			return sig == kSignature;
		}
	};
}