using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Resource
{
	using Adler32 = Security.Cryptography.Adler32;

	public sealed partial class CompressedStream
		: IO.IEndianStreamSerializable
		, IDisposable
	{
		enum Mode : uint
		{
			Streaming,
			Buffered,
			BufferedEnd,
		};

		/// <summary>Max size of a chunk of a in a buffered stream</summary>
		const int kBufferedSize = 0x2000; // cOutBufSize

		public const uint kSignature = 0xCC34EEAD;
		const uint kSignatureEndOfStream = 0xA5D91776;

		Header mHeader;
		bool mUseBufferedStreaming;

		public byte[] CompressedData { get; private set; }
		public byte[] UncompressedData { get; private set; }

		public CompressedStream(bool useBufferedStreaming = false)
		{
			this.mUseBufferedStreaming = useBufferedStreaming;
		}

		#region IDisposable Members
		public void Dispose()
		{
			if (this.UncompressedData != null)
				this.UncompressedData = null;

			if (this.CompressedData != null)
				this.CompressedData = null;
		}
		#endregion

		#region IEndianStreamSerializable Members
		void StreamCompressedData(IO.EndianStream s)
		{
			if (s.IsReading)
				this.CompressedData = new byte[(int) this.mHeader.CompressedSize];

			s.Stream(this.CompressedData);
			s.StreamSignature(kSignatureEndOfStream);
		}

		#region Chunk buffering
		int ReadChunk(IO.EndianReader s, System.IO.MemoryStream ms)
		{
			ushort chunk_size = s.ReadUInt16(), negate_chunk_size = s.ReadUInt16();

			ushort expected_negate = (ushort)~chunk_size;
			if (expected_negate != negate_chunk_size)
			{
				throw new IO.SignatureMismatchException(s.BaseStream,
					expected_negate, negate_chunk_size);
			}

			byte[] bytes = s.ReadBytes(chunk_size);
			ms.Write(bytes, 0, bytes.Length);

			return chunk_size;
		}
		int WriteChunk(IO.EndianWriter s, int chunkStart, ref int bytesRemaining)
		{
			if (chunkStart == this.CompressedData.Length)
			{
				s.Write((ushort)0x0000);
				s.Write((ushort)0xFFFF); // ~0
				return 0;
			}

			int chunk_size = (bytesRemaining < kBufferedSize)
				? this.CompressedData.Length % kBufferedSize
				: kBufferedSize;
			bytesRemaining -= chunk_size;

			s.Write((ushort)chunk_size);
			s.Write((ushort)~chunk_size);
			s.Write(this.CompressedData, chunkStart, chunk_size);

			return chunk_size;
		}
		void ReadCompressedDataInChunks(IO.EndianStream s, int initBufferCapacity)
		{
			using (var ms = new System.IO.MemoryStream(initBufferCapacity))
			{
				while (this.ReadChunk(s.Reader, ms) != 0)
				{
				}
				s.StreamSignature(kSignatureEndOfStream);

				this.CompressedData = ms.ToArray();
			}
		}
		void WriteCompressedDataInChunks(IO.EndianStream s)
		{
			for (int offset = 0, size = 0, bytes_remaining = this.CompressedData.Length;
				(size = this.WriteChunk(s.Writer, offset, ref bytes_remaining)) != 0;
				offset += size)
			{
			}

			s.StreamSignature(kSignatureEndOfStream);
		}
		void StreamCompressedDataInChunks(IO.EndianStream s, int initBufferCapacity = 4096)
		{
			if (s.IsReading)
			{
				this.ReadCompressedDataInChunks(s, initBufferCapacity);
			}
			else if (s.IsWriting)
			{
				this.WriteCompressedDataInChunks(s);
			}
		}
		#endregion

		public void Serialize(IO.EndianStream s)
		{
			bool writing = s.IsWriting;

			if (s.IsReading)
			{
				s.Stream(ref this.mHeader);

				this.mHeader.UpdateHeaderCrc();
				this.mUseBufferedStreaming = this.mHeader.UseBufferedStreaming;
			}
			else if (writing)
			{
				var head = this.mUseBufferedStreaming
					? kBufferedHeader
					: this.mHeader;
				s.Stream(ref head);
			}

			if (!this.mUseBufferedStreaming)
			{
				Contract.Assert(!writing || this.mHeader.StreamMode == (uint)Mode.Streaming);

				this.StreamCompressedData(s);
			}
			else
			{
				Contract.Assert(!writing || this.mHeader.StreamMode == (uint)Mode.Buffered);

				this.StreamCompressedDataInChunks(s);
				s.Stream(ref this.mHeader); // actual header appears after the chunks

				Contract.Assert(!writing || this.mHeader.StreamMode == (uint)Mode.BufferedEnd);
			}
		}
		#endregion

		public void ReadData(System.IO.Stream s)
		{
			this.UncompressedData = new byte[this.mHeader.UncompressedSize];
			s.Read(this.UncompressedData, 0, this.UncompressedData.Length);

			this.mHeader.UncompressedAdler32 = Adler32.Compute(this.UncompressedData);
		}
		public void WriteData(System.IO.Stream s)
		{
			s.Write(this.UncompressedData, 0, this.UncompressedData.Length);
		}
		public void InitializeFromStream(System.IO.Stream source)
		{
			Contract.Requires(source.CanRead);

			this.mHeader.UncompressedSize = (ulong)source.Length;
			this.ReadData(source);
		}

		public void Compress(int level = 5)
		{
			Contract.Requires(level.IsNone() ||
				(level >= IO.Compression.ZLib.kNoCompression && level <= IO.Compression.ZLib.kBestCompression));

			// Assume the compressed data will be at most the same size as the uncompressed data
			if (this.CompressedData == null || this.CompressedData.Length < (int) this.mHeader.UncompressedSize)
			{
				this.CompressedData = new byte[this.mHeader.UncompressedSize];
			}
			else
			{
				Array.Clear(this.CompressedData, 0, this.CompressedData.Length);
			}

			uint adler32;
			this.CompressedData = IO.Compression.ZLib.LowLevelCompress(this.UncompressedData, level,
			                                                        out adler32/*mHeader.CompressedAdler32*/,
			                                                        this.CompressedData);

			this.mHeader.CompressedAdler32 = Adler32.Compute(this.CompressedData);
			if (this.mHeader.CompressedAdler32 != adler32)
			{
#if false
				Debug.Trace.Resource.TraceInformation("ZLib.LowLevelCompress returned different adler32 ({0}) than our computations ({1}). Uncompressed adler32={2}",
					adler32.ToString("X8"),
					mHeader.CompressedAdler32.ToString("X8"),
					mHeader.UncompressedAdler32.ToString("X8"));
#endif
			}

			this.mHeader.CompressedSize = (ulong) this.CompressedData.LongLength;
		}
		public void Decompress()
		{
			if (this.UncompressedData == null || this.UncompressedData.Length < (int) this.mHeader.UncompressedSize)
			{
				this.UncompressedData = new byte[this.mHeader.UncompressedSize];
			}
			else
			{
				Array.Clear(this.UncompressedData, 0, this.UncompressedData.Length);
			}

			IO.Compression.ZLib.LowLevelDecompress(this.CompressedData, this.UncompressedData);
		}
	};
}