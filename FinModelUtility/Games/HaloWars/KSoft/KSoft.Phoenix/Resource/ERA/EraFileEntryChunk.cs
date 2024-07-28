using System;

namespace KSoft.Phoenix.Resource
{
	public sealed class EraFileEntryChunk
		: ECF.EcfChunk
	{
		#region SizeOf
		public new const int kSizeOf = 0x38;
		public const int kCompresssedDataTigerHashSize = 16;
		public const uint kExtraDataSize = kSizeOf - ECF.EcfChunk.kSizeOf;

		public static int CalculateFileChunksSize(int fileCount)
		{
			return kSizeOf * fileCount;
		}
		#endregion

		#region Struct fields
		private ulong mFileTimeBits; // FILETIME
		public int DataUncompressedSize;
		// First 128 bits of the compressed data's Tiger hash
		public byte[] CompressedDataTiger128 = new byte[kCompresssedDataTigerHashSize];
		// actually only 24 bits, big endian
		public uint FileNameOffset;
		#endregion

		public string FileName;
		public DateTime FileDateTime
		{
			get { return DateTime.FromFileTimeUtc((long) this.mFileTimeBits); }
			set { this.mFileTimeBits = (ulong)value.ToFileTimeUtc(); }
		}

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			var eraUtil = s.Owner as EraFileUtil;
			long position = s.BaseStream.Position;

			base.Serialize(s);

			s.Stream(ref this.mFileTimeBits);
			s.Stream(ref this.DataUncompressedSize);

			if (s.IsWriting)
			{
				Bitwise.ByteSwap.SwapInt64(this.CompressedDataTiger128, sizeof(ulong) * 0);
				Bitwise.ByteSwap.SwapInt64(this.CompressedDataTiger128, sizeof(ulong) * 1);
			}
			s.Stream(this.CompressedDataTiger128);
			{
				Bitwise.ByteSwap.SwapInt64(this.CompressedDataTiger128, sizeof(ulong) * 0);
				Bitwise.ByteSwap.SwapInt64(this.CompressedDataTiger128, sizeof(ulong) * 1);
			}


			s.StreamUInt24(ref this.FileNameOffset);
			s.Pad8();

			if (eraUtil != null && eraUtil.DebugOutput != null)
			{
				eraUtil.DebugOutput.Write("FileEntry: {0} @{1} offset={2} end={3} size={4} dsize={5} adler={6} ",
					this.EntryId.ToString("X16"),
					position.ToString("X8"),
					this.DataOffset.u32.ToString("X8"),
					(this.DataOffset.u32 + this.DataSize).ToString("X8"),
					this.DataSize.ToString("X8"),
					this.DataUncompressedSize.ToString("X8"),
					this.Adler32.ToString("X8"));

				if (!string.IsNullOrEmpty(this.FileName))
					eraUtil.DebugOutput.Write(this.FileName);

				eraUtil.DebugOutput.WriteLine();
			}
		}
		#endregion

		#region Xml Streaming
		string FileDateTimeString { get {
			return this.FileDateTime.ToString("u"); // UniversalSorta­bleDateTimePat­tern
		} }

		protected override void WriteFields(IO.XmlElementStream s, bool includeFileData)
		{
			if (includeFileData && this.mFileTimeBits != 0)
				s.WriteAttribute("fileTime", this.mFileTimeBits.ToString("X16"));

			// only because it's interesting to have, never read back in
			s.WriteAttribute("fileDateTime", this.FileDateTimeString);

			base.WriteFields(s, includeFileData);

			// When we extract, we decode xmbs
			string fn = this.FileName;
			if (ResourceUtils.IsXmbFile(fn))
			{
				bool remove_xmb_ext = true;

				var expander = s.Owner as EraFileExpander;
				if (expander != null && expander.ExpanderOptions.Test(EraFileExpanderOptions.DontTranslateXmbFiles))
					remove_xmb_ext = false;

				if (remove_xmb_ext)
					ResourceUtils.RemoveXmbExtension(ref fn);
			}
			s.WriteAttribute("name", fn);

			if (includeFileData)
			{
				if (this.DataUncompressedSize != this.DataSize)
					s.WriteAttribute("fullSize", this.DataUncompressedSize.ToString("X8"));

				s.WriteAttribute("compressedDataHash",
					Text.Util.ByteArrayToString(this.CompressedDataTiger128));

				s.WriteAttribute("nameOffset", this.FileNameOffset.ToString("X6"));
			}

			this.WriteFlags(s);
			this.WriteResourceFlags(s);
		}

		protected override void ReadFields(IO.XmlElementStream s, bool includeFileData)
		{
			base.ReadFields(s, includeFileData);

			s.ReadAttributeOpt("fileTime", ref this.mFileTimeBits, NumeralBase.Hex);

			s.ReadAttribute("name", ref this.FileName);

			s.ReadAttributeOpt("fullSize", ref this.DataUncompressedSize, NumeralBase.Hex);
			s.ReadAttributeOpt("nameOffset", ref this.FileNameOffset, NumeralBase.Hex);

			string hashString = null;
			if (s.ReadAttributeOpt("compressedDataHash", ref hashString))
				this.CompressedDataTiger128 = Text.Util.ByteStringToArray(hashString);
		}
		#endregion

		#region Buffer Util
		protected override byte[] DecompressFromBuffer(IO.EndianStream blockStream, byte[] buffer)
		{
			uint result_adler;
			return ResourceUtils.Decompress(buffer, this.DataUncompressedSize, out result_adler);
		}

		public override void BuildBuffer(IO.EndianStream blockStream, System.IO.Stream sourceFile,
			Security.Cryptography.TigerHashBase hasher)
		{
			base.BuildBuffer(blockStream, sourceFile, hasher);

			this.ComputeHash(blockStream, hasher);
			Array.Copy(hasher.Hash, 0, this.CompressedDataTiger128, 0, this.CompressedDataTiger128.Length);
		}

		protected override void CompressSourceToStream(IO.EndianWriter blockStream, System.IO.Stream sourceFile)
		{
			this.DataUncompressedSize = (int)sourceFile.Length;

			base.CompressSourceToStream(blockStream, sourceFile);
		}
		#endregion

		public override string ToString()
		{
			return string.Format("{0}",
			                     this.FileName);
		}
	};
}