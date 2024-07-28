
namespace KSoft.Phoenix.Resource
{
	/*public*/ sealed class EraFileHeader
		: IO.IEndianStreamSerializable
	{
		const uint kSiganture = 0x17FDBA9C;
		const int kHeaderSize = 0x1E00;

		public static int CalculateHeaderSize() { return kHeaderSize; }

		ECF.EcfHeader mHeader;
		EraFileSignature mSignature = new EraFileSignature();

		public int FileCount { get { return this.mHeader.ChunkCount; } }

		public EraFileHeader()
		{
			this.mHeader.InitializeChunkInfo(kSiganture, EraFileEntryChunk.kExtraDataSize);
			this.mHeader.HeaderSize = kHeaderSize;
		}

		public void UpdateFileCount(int count)
		{
			this.mHeader.ChunkCount = (short)count;
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			var eraFile = s.Owner as EraFileUtil;

			if (s.IsWriting)
			{
				this.mHeader.UpdateTotalSize(s.BaseStream);
			}

			long header_position = s.BaseStream.CanSeek
				? s.BaseStream.Position
				: -1;

			// write the header, but it won't have the correct CRC if things have changed,
			// or if this is a fresh new archive
			this.mHeader.Serialize(s);
			this.mSignature.Serialize(s);

			var leftovers_size = this.mHeader.HeaderSize - s.BaseStream.Position;
			s.Pad((int)leftovers_size);

			// verify or update the header checksum
			if (s.IsReading)
			{
				if (header_position != -1 &&
					!eraFile.Options.Test(EraFileUtilOptions.SkipVerification))
				{
					var actual_adler = this.mHeader.ComputeAdler32(s.BaseStream, header_position);
					if (actual_adler != this.mHeader.Adler32)
					{
						throw new System.IO.InvalidDataException(string.Format(
							"ERA header adler32 {0} does not match actual adler32 {1}",
							this.mHeader.Adler32.ToString("X8"),
							actual_adler.ToString("X8")
							));
					}
				}
			}
			else if (s.IsWriting)
			{
				if (header_position != -1)
				{
					this.mHeader.ComputeAdler32AndWrite(s, header_position);
				}
			}
		}
		#endregion

		public static bool VerifyIsEraAndDecrypted(IO.EndianReader s)
		{
			return ECF.EcfHeader.VerifyIsEcf(s);
		}
	};
}