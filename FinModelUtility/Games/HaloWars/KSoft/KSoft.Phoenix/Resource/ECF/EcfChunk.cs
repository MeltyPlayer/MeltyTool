using System;
using System.IO;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Resource.ECF
{
	public enum EcfCompressionType : byte
	{
		Stored,
		/// <summary>File data is stored in a standard compressed buffer (no header/footer/etc data, just plain old compressed bytes)</summary>
		DeflateRaw,
		/// <summary>File data is stored in a <see cref="CompressedStream"/> buffer</summary>
		DeflateStream,
	};

	enum EcfChunkResourceFlags : ushort
	{
		Contiguous,
		WriteCombined, // only valid with Contiguous
		IsDeflateStream,
		IsResourceTag,
	};

	public class EcfChunk
		: IO.IEndianStreamSerializable
	{
		public const int kMaxCount = 32768;
		public const uint kMaxSize = 1024U * 1024U * 1024U;

		public const int kSizeOf = 0x18;
		internal const string kXmlElementStreamName = "chunk";

		public const int kDefaultAlignmentBit = 2;
		const byte kCompressionTypeMask = 7;

		#region Struct fields
		public ulong EntryId;
		public Values.PtrHandle DataOffset = Values.PtrHandle.Null32; // offset within the parent block
		public int DataSize;
		public uint Adler32;
		public byte Flags;
		public byte DataAlignmentBit = kDefaultAlignmentBit;
		private ushort mResourceFlags;
		#endregion

		public ulong DecompressedDataTiger64
		{
			get { return this.EntryId; }
			set { this.EntryId = value; }
		}

		public EcfCompressionType CompressionType
		{
			get { return (EcfCompressionType)(this.Flags & kCompressionTypeMask); }
			set { this.Flags |= (byte)( ((byte)(value)) & kCompressionTypeMask ); }
		}

		#region Resource flags
		public bool IsContiguous
		{
			get { return Bitwise.Flags.Test(this.mResourceFlags, 1U<<(ushort)EcfChunkResourceFlags.Contiguous); }
			set { Bitwise.Flags.Modify(value, ref this.mResourceFlags, (ushort)1U<<(ushort)EcfChunkResourceFlags.Contiguous); }
		}

		public bool IsWriteCombined
		{
			get { return Bitwise.Flags.Test(this.mResourceFlags, 1U<<(ushort)EcfChunkResourceFlags.WriteCombined); }
			set { Bitwise.Flags.Modify(value, ref this.mResourceFlags, (ushort)1U<<(ushort)EcfChunkResourceFlags.WriteCombined); }
		}

		public bool IsDeflateStream
		{
			get { return Bitwise.Flags.Test(this.mResourceFlags, 1U<<(ushort)EcfChunkResourceFlags.IsDeflateStream); }
			set { Bitwise.Flags.Modify(value, ref this.mResourceFlags, (ushort)1U<<(ushort)EcfChunkResourceFlags.IsDeflateStream); }
		}

		public bool IsResourceTag
		{
			get { return Bitwise.Flags.Test(this.mResourceFlags, 1U<<(ushort)EcfChunkResourceFlags.IsResourceTag); }
			set { Bitwise.Flags.Modify(value, ref this.mResourceFlags, (ushort)1U<<(ushort)EcfChunkResourceFlags.IsResourceTag); }
		}
		#endregion

		/// <summary>
		/// Flag is set but CompressionType is Stored. This will be the case with XMBs.
		/// </summary>
		public bool IsDeflateStreamButNoCompression { get {
			return this.IsDeflateStream && this.CompressionType == EcfCompressionType.Stored;
		} }

		public void SeekTo(IO.IKSoftBinaryStream blockStream)
		{
			blockStream.Seek((long) this.DataOffset);
		}
		public byte[] GetRawBuffer(IO.EndianStream blockStream)
		{
			this.SeekTo(blockStream);
			byte[] result = blockStream.Reader.ReadBytes(this.DataSize);

			return result;
		}

		#region Buffer Util
		public byte[] GetBuffer(IO.EndianStream blockStream)
		{
			byte[] result = null;

			var assumed_compression_type = this.CompressionType;

			// #NOTE CompressionType can be Stored but IsDeflateStream can be true (seen it in XMB).
			// So just handle the flag as we do EcfCompressionType.DeflateStream
			if (this.IsDeflateStream)
			{
				assumed_compression_type = EcfCompressionType.DeflateStream;
			}

			switch (assumed_compression_type)
			{
				case EcfCompressionType.Stored:
					result = this.GetRawBuffer(blockStream);
					break;

				case EcfCompressionType.DeflateRaw:
					result = this.GetRawBuffer(blockStream);
					result = this.DecompressFromBuffer(blockStream, result);
					break;

				case EcfCompressionType.DeflateStream:
					result = this.DecompressFromStream(blockStream);
					break;

				default:
					throw new KSoft.Debug.UnreachableException(assumed_compression_type.ToString());
			}

			return result;
		}

		protected virtual byte[] DecompressFromBuffer(IO.EndianStream blockStream, byte[] buffer)
		{
			throw new InvalidOperationException(string.Format(
				"Can't get the decompressed bytes for {0} (from {1}). Need to know the uncompressed data size",
				this.EntryId, blockStream.StreamName));
		}

		byte[] DecompressFromStream(IO.EndianStream blockStream)
		{
			this.SeekTo(blockStream);
			return CompressedStream.DecompressFromStream(blockStream);
		}

		public virtual void BuildBuffer(IO.EndianStream blockStream, Stream sourceFile
			, Security.Cryptography.TigerHashBase hasher = null)
		{
			blockStream.AlignToBoundry(this.DataAlignmentBit);

			sourceFile.Seek(0, SeekOrigin.Begin);
			if (hasher != null)
				this.UpdateDecompressedDataTigerHash(sourceFile, hasher);

			Contract.Assert(blockStream.BaseStream.Position == blockStream.BaseStream.Length);

			this.DataOffset = blockStream.PositionPtr;

			// #TODO determine if compressing the sourceFile data has any savings (eg, 7% smaller)

			var assumed_compression_type = this.CompressionType;

			// #NOTE CompressionType can be Stored but IsDeflateStream can be true (seen it in XMB).
			// So just handle the flag as we do EcfCompressionType.DeflateStream
			if (this.IsDeflateStream)
			{
				assumed_compression_type = EcfCompressionType.DeflateStream;
			}

			switch (assumed_compression_type)
			{
				case EcfCompressionType.Stored:
				{
					// Update this ECF's size
					this.DataSize = (int)sourceFile.Length;
					// Also update this ECF's checksum
					this.Adler32 = Security.Cryptography.Adler32.Compute(sourceFile, this.DataSize, restorePosition: true);
					// Copy the source file's bytes to the block stream
					sourceFile.CopyTo(blockStream.BaseStream);
					break;
				}

				case EcfCompressionType.DeflateRaw:
					this.CompressSourceToStream(blockStream.Writer, sourceFile);
					break;

				case EcfCompressionType.DeflateStream:
					this.CompressSourceToCompressionStream(blockStream.Writer, sourceFile);
					break;

				default:
					throw new KSoft.Debug.UnreachableException(assumed_compression_type.ToString());
			}

			Contract.Assert(blockStream.BaseStream.Position == ((long) this.DataOffset + this.DataSize));
		}

		protected virtual void CompressSourceToStream(IO.EndianWriter blockStream, Stream sourceFile)
		{
			// Read the source bytes
			byte[] buffer = new byte[sourceFile.Length];
			for (int x = 0; x < buffer.Length;)
			{
				int n = sourceFile.Read(buffer, x, buffer.Length - x);
				x += n;
			}

			// Compress the source bytes into a new buffer
			byte[] result = ResourceUtils.Compress(buffer, out this.Adler32); // Also update this ECF's checksum
			this.DataSize = result.Length;                                       // Update this ECF's size

			// Write the compressed bytes to the block stream
			blockStream.Write(result);
		}

		protected void CompressSourceToCompressionStream(IO.EndianWriter blockStream, Stream sourceFile)
		{
			// Build a CompressedStream from the source file and write it to the block stream
			CompressedStream.CompressFromStream(blockStream, sourceFile,
				out this.Adler32, out this.DataSize);  // Update this ECF's checksum and size
		}
		#endregion

		#region Hash Utils
		[Contracts.Pure]
		public uint ComputeAdler32(IO.EndianStream blockStream)
		{
			Contract.Requires(blockStream != null);

			this.SeekTo(blockStream);
			uint adler = Security.Cryptography.Adler32.Compute(blockStream.BaseStream, this.DataSize);
			return adler;
		}

		[Contracts.Pure]
		public void ComputeHash(IO.EndianStream blockStream, Security.Cryptography.TigerHashBase hasher)
		{
			Contract.Requires(blockStream != null);
			Contract.Requires(hasher != null);

			hasher.Initialize();
			hasher.ComputeHash(blockStream.BaseStream,
				(long) this.DataOffset,
				this.DataSize);
		}

		protected void UpdateDecompressedDataTigerHash(Stream source, Security.Cryptography.TigerHashBase hasher)
		{
			hasher.ComputeHash(source, 0, (int)source.Length,
				restorePosition: true);

			ulong tiger64;
			hasher.TryGetAsTiger64(out tiger64);
			this.DecompressedDataTiger64 = tiger64;
		}
		#endregion

		#region IEndianStreamSerializable Members
		public virtual void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.EntryId);
			s.StreamVirtualAddress(ref this.DataOffset);
			s.Stream(ref this.DataSize);
			s.Stream(ref this.Adler32);
			s.Stream(ref this.Flags);
			s.Stream(ref this.DataAlignmentBit);
			s.Stream(ref this.mResourceFlags);
		}
		#endregion

		#region Xml Streaming
		public void Read(IO.XmlElementStream s, bool includeFileData)
		{
			this.ReadFields(s, includeFileData);
		}
		public void Write(IO.XmlElementStream s, bool includeFileData)
		{
			using (s.EnterCursorBookmark(kXmlElementStreamName))
				this.WriteFields(s, includeFileData);
		}

		protected virtual void ReadFields(IO.XmlElementStream s, bool includeFileData)
		{
			s.ReadAttributeOpt("id", ref this.EntryId, NumeralBase.Hex);
			//s.ReadAttributeOpt("flags", ref Flags, NumeralBase.Hex);
			s.ReadAttributeOpt("align", ref this.DataAlignmentBit, NumeralBase.Hex);
			if (includeFileData)
			{
				s.ReadAttributeOpt("offset", ref this.DataOffset.u32, NumeralBase.Hex);
				s.ReadAttributeOpt("size", ref this.DataSize, NumeralBase.Hex);
			}

			this.ReadFlags(s);
			this.ReadResourceFlags(s);
		}
		protected virtual void WriteFields(IO.XmlElementStream s, bool includeFileData)
		{
			s.WriteAttribute("id", this.EntryId.ToString("X16"));
			//if (Flags != 0)
			//	s.WriteAttribute("flags", Flags.ToString("X1"));
			if (this.DataAlignmentBit != kDefaultAlignmentBit)
				s.WriteAttribute("align", this.DataAlignmentBit.ToString("X1"));
			if (includeFileData)
			{
				s.WriteAttribute("offset", this.DataOffset.u32.ToString("X8"));
				s.WriteAttribute("size", this.DataSize.ToString("X8"));
			}
		}

		protected void ReadFlags(IO.XmlElementStream s)
		{
			var compType = EcfCompressionType.Stored;
			if (s.ReadAttributeEnumOpt("Compression", ref compType))
				this.CompressionType = compType;
		}
		protected void WriteFlags(IO.XmlElementStream s)
		{
			if (this.Flags == 0)
				return;

			s.WriteAttributeEnum("Compression", this.CompressionType);
		}

		protected void ReadResourceFlags(IO.XmlElementStream s)
		{
			bool flag = false;
			if (s.ReadAttributeOpt("IsContiguous", ref flag))
				this.IsContiguous = flag;
			if (s.ReadAttributeOpt("IsWriteCombined", ref flag))
				this.IsWriteCombined = flag;
			if (s.ReadAttributeOpt("IsDeflateStream", ref flag))
				this.IsDeflateStream = flag;
			if (s.ReadAttributeOpt("IsResourceTag", ref flag))
				this.IsResourceTag = flag;
		}
		protected void WriteResourceFlags(IO.XmlElementStream s)
		{
			if (this.mResourceFlags == 0)
				return;

			if (this.IsContiguous)
				s.WriteAttribute("IsContiguous", true);
			if (this.IsWriteCombined)
				s.WriteAttribute("IsWriteCombined", true);
			if (this.IsDeflateStream)
				s.WriteAttribute("IsDeflateStream", true);
			if (this.IsResourceTag)
				s.WriteAttribute("IsResourceTag", true);
		}
		#endregion
	};
}