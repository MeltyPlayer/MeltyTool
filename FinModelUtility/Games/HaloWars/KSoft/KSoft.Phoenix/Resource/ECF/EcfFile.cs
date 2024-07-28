using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
using System.Collections;

namespace KSoft.Phoenix.Resource.ECF
{
	// http://en.wikipedia.org/wiki/Unix_File_System

	public class EcfFile
		: IDisposable
		, IO.IEndianStreamSerializable
		, IEnumerable<EcfChunk>
	{
		internal EcfHeader mHeader = new EcfHeader();
		protected List<EcfChunk> mChunks = new List<EcfChunk>();

		public int ChunksCount { get { return this.mChunks.Count; } }

		public EcfFile()
		{
			this.mHeader.HeaderSize = EcfHeader.kSizeOf;
		}

		public void InitializeChunkInfo(uint dataId, uint dataChunkExtraDataSize = 0)
		{
			this.mHeader.InitializeChunkInfo(dataId, dataChunkExtraDataSize);
		}

		public int CalculateHeaderAndChunkEntriesSize()
		{
			return this.mHeader.HeaderSize +
			       this.mHeader.CalculateChunkEntriesSize(this.ChunksCount);
		}

		#region IDisposable Members
		public virtual void Dispose()
		{
			this.mChunks.Clear();
		}
		#endregion

		#region IEndianStreamSerializable Members
		public virtual void Serialize(IO.EndianStream s)
		{
			this.SerializeBegin(s);
			this.SerializeChunkHeaders(s);
			this.SerializeEnd(s);
		}

		internal void SerializeBegin(IO.EndianStream s
			, bool isFinalizing = false)
		{
			var ecfFile = s.Owner as EcfFileUtil;

			if (s.IsWriting)
			{
				this.mHeader.ChunkCount = (short) this.mChunks.Count;

				if (isFinalizing)
					this.mHeader.UpdateTotalSize(s.BaseStream);
			}

			long header_position = s.BaseStream.CanSeek
				? s.BaseStream.Position
				: -1;

			this.mHeader.BeginBlock(s);
			this.mHeader.Serialize(s);

			// verify or update the header checksum
			if (s.IsReading)
			{
				if (header_position != -1 &&
					ecfFile != null &&
					!ecfFile.Options.Test(EraFileUtilOptions.SkipVerification))
				{
					var actual_adler = this.mHeader.ComputeAdler32(s.BaseStream, header_position);
					if (actual_adler != this.mHeader.Adler32)
					{
						throw new System.IO.InvalidDataException(string.Format(
							"ECF header adler32 {0} does not match actual adler32 {1}",
							this.mHeader.Adler32.ToString("X8"),
							actual_adler.ToString("X8")
							));
					}
				}
			}
			else if (s.IsWriting)
			{
				if (header_position != -1 && isFinalizing)
				{
					this.mHeader.ComputeAdler32AndWrite(s, header_position);
				}
			}
		}

		internal void SerializeChunkHeaders(IO.EndianStream s)
		{
			s.StreamListElementsWithClear(this.mChunks, this.mHeader.ChunkCount, () => new EcfChunk());
		}

		internal void SerializeEnd(IO.EndianStream s)
		{
			this.mHeader.EndBlock(s);
		}
		#endregion

		#region Chunk accessors
		public IEnumerator<EcfChunk> GetEnumerator()
		{
			return ((IEnumerable<EcfChunk>) this.mChunks).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<EcfChunk>) this.mChunks).GetEnumerator();
		}

		public EcfChunk GetChunk(int chunkIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(chunkIndex >= 0 && chunkIndex < this.ChunksCount);

			return this.mChunks[chunkIndex];
		}
		#endregion

		public void CopyHeaderDataTo(EcfFileDefinition definition)
		{
			definition.CopyHeaderData(this.mHeader);
		}

		public void SetupHeaderAndChunks(EcfFileDefinition definition)
		{
			this.mChunks.Clear();

			definition.UpdateHeader(ref this.mHeader);
			foreach (var chunk in definition.Chunks)
			{
				var rawChunk = new EcfChunk();
				chunk.SetupRawChunk(rawChunk, this.ChunksCount);
				this.mChunks.Add(rawChunk);
			}
		}
	};
}