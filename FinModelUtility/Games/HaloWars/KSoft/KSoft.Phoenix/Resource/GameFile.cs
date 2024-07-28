using System;
using System.Security.Cryptography;

namespace KSoft.Phoenix.Resource
{
	using PhxHash = Security.Cryptography.PhxHash;

	using FileFlagsStreamer = IO.EnumBinaryStreamer<GameFile.FileFlags, ushort>;

	public sealed class GameFile
		: IDisposable
		, IO.IEndianStreamSerializable
	{
	[Flags]
		internal enum FileFlags : ushort
		{
			CompressContent = 1<<0,
			EncryptContent = 1<<1,
			EncryptHeader = 1<<2,

			kAll = CompressContent | EncryptContent | EncryptHeader,
		};
		const ushort kVersion = 0x17;

		const int kMaxContentSize = 0xDD240;
		/// <summary>Number of random words that follow the content payload</summary>
		const int kRandomBlockWords = 0xA00;

		FileFlags Flags = FileFlags.kAll;
		internal SHA1CryptoServiceProvider ShaContext { get; set; } = new SHA1CryptoServiceProvider();

		public MediaHeader Header { get; private set; } = new MediaHeader();

		public byte[] Content { get; set; }
		byte[] PaddingBytes { get; set; }

		public void GenerateHash()
		{
			ShaContext.Initialize();

			PhxHash.UInt16(ShaContext, (ushort)Flags);
			PhxHash.UInt16(ShaContext, kVersion);

			Header.UpdateHash(ShaContext);
		}

		static uint WriteRandomBlock(IO.EndianWriter s, uint seed = 1)
		{
			for (int x = kRandomBlockWords; x > 0; x--)
			{
				uint r8 = seed << 17;
				uint r7 = r8 ^ seed;
				uint r6 = r7 >> 13;
				uint r5 = r6 ^ r7;
				uint r4 = r5 << 5;
				seed = r4 ^ r5;
				s.Write(seed);
			}

			return seed;
		}

	static void Stream(IO.EndianStream s, bool crypt, IO.IEndianStreamSerializable obj,
			long size = 0, ulong userKey = 0, Action<IO.EndianStream> streamLeftovers = null)
		{
			if(!crypt)
			{
				obj.Serialize(s);

				if (s.IsWriting && streamLeftovers != null)
					streamLeftovers(s);
			}
			else
			{
				using (var ms = new System.IO.MemoryStream())
				using (var crypted = new IO.EndianStream(ms, s.ByteOrder))
				{
					crypted.StreamMode = s.StreamMode;

					if (s.IsReading)
					{
						var tea = new Security.Cryptography.PhxTEA(s.Reader, crypted.Writer);
						tea.InitializeKey(Security.Cryptography.PhxTEA.kKeyGameFile, userKey);
						tea.Decrypt(size);

						crypted.Seek(0);
					}

					obj.Serialize(crypted);

					if (streamLeftovers != null)
						streamLeftovers(crypted);

					if (s.IsWriting)
					{
						crypted.Seek(0);

						var tea = new Security.Cryptography.PhxTEA(crypted.Reader, s.Writer);
						tea.InitializeKey(Security.Cryptography.PhxTEA.kKeyGameFile, userKey);
						tea.Encrypt(size);
					}
				}
			}
		}

	public void Dispose()
		{
			if (ShaContext != null)
			{
				ShaContext.Dispose();
				ShaContext = null;
			}
		}

		#region IEndianStreamSerializable Members
		void ReadLeftovers(IO.EndianReader er)
		{
			PaddingBytes = new byte[(int)(er.BaseStream.Length - er.BaseStream.Position)];
			er.Read(PaddingBytes, 0, PaddingBytes.Length);
		}
		void WriteLeftovers(IO.EndianWriter ew)
		{
//			WriteRandomBlocks(ew);

			if (ew.BaseStream.Length < kMaxContentSize)
			{
				int padding_bytes_count = System.Math.Min(PaddingBytes.Length, kMaxContentSize - (int)(ew.BaseStream.Length));
				ew.Write(PaddingBytes, 0, padding_bytes_count);
			}

			if (ew.BaseStream.Length < kMaxContentSize)
			{
				byte[] zero = new byte[kMaxContentSize - (int)(ew.BaseStream.Length)];
				Array.Clear(zero, 0, zero.Length);
				ew.Write(zero, 0, zero.Length);
			}
		}
		void StreamLeftovers(IO.EndianStream s)
		{
				 if (s.IsReading) ReadLeftovers(s.Reader);
			else if (s.IsWriting) WriteLeftovers(s.Writer);
		}
		void StreamCompressedContent(IO.EndianStream s)
		{
			if (s.IsReading)
			{
				using (var cs = new CompressedStream(true))
				{
					Stream(s, EnumFlags.Test(Flags, FileFlags.EncryptContent), cs,
						userKey: Header.DataCryptKey, streamLeftovers: StreamLeftovers);

					cs.Decompress();
					Content = cs.UncompressedData;
				}
			}
			else if (s.IsWriting)
			{
				using (var cs = new CompressedStream(true))
				using (var ms = new System.IO.MemoryStream(kMaxContentSize))
				using (var sout = new IO.EndianWriter(ms, s.ByteOrder))
				{
					sout.Write(Content);
					sout.Seek(0);

					cs.InitializeFromStream(ms);
					cs.Compress();

					Stream(s, EnumFlags.Test(Flags, FileFlags.EncryptContent), cs,
						userKey: Header.DataCryptKey, streamLeftovers: StreamLeftovers);
				}
			}
		}
		public void Serialize(IO.EndianStream s)
		{
			s.Owner = this;

			if (s.IsWriting)
			{
				//Flags = EnumFlags.Remove(Flags, FileFlags.EncryptHeader | FileFlags.EncryptContent);
			}

			s.Stream(ref Flags, FileFlagsStreamer.Instance);
			s.StreamVersion(kVersion);

			Stream(s, EnumFlags.Test(Flags, FileFlags.EncryptHeader), Header, MediaHeader.kSizeOf);
			GenerateHash();

			if (EnumFlags.Test(Flags, FileFlags.CompressContent))
			{
				StreamCompressedContent(s);
			}
			else
			{
				if (s.IsReading)
					Content = new byte[(int)(s.BaseStream.Length - s.BaseStream.Position)];

				s.Stream(Content);
			}
		}
		#endregion

		#region IEndianStreamable Members
#if false // TODO: verify the new IEndianStreamSerializable impl
		public void Read(IO.EndianReader s)
		{
			s.Owner = this;

			Flags = s.Read(FileFlagsStreamer.Instance);
			Version = s.ReadUInt16();
			if (Version != kVersion) throw new IO.VersionMismatchException(s.BaseStream,
				kVersion, Version);

			Read(s, EnumFlags.Test(Flags, FileFlags.EncryptHeader), Header, MediaHeader.kSizeOf);
			GenerateHash();

			if (EnumFlags.Test(Flags, FileFlags.CompressContent))
			{
				using (var cs = new CompressedStream(true))
				{
					Read(s, EnumFlags.Test(Flags, FileFlags.EncryptContent), cs,
						userKey: Header.DataCryptKey, readLeftovers: ReadLeftovers);

					cs.Decompress();
					Content = cs.UncompressedData;
				}
			}
			else
				Content = s.ReadBytes((int)(s.BaseStream.Length - s.BaseStream.Position));
		}
		public void Write(IO.EndianWriter s)
		{
			//Flags = EnumFlags.Remove(Flags, FileFlags.EncryptHeader | FileFlags.EncryptContent);

			s.Write(Flags, FileFlagsStreamer.Instance);
			s.Write((ushort)kVersion);

			Write(s, EnumFlags.Test(Flags, FileFlags.EncryptHeader), Header, MediaHeader.kSizeOf);
			GenerateHash();

			if (EnumFlags.Test(Flags, FileFlags.CompressContent))
			{
				using (var cs = new CompressedStream(true))
				using (var ms = new System.IO.MemoryStream(kMaxContentSize))
				using (var sout = new IO.EndianWriter(ms, Shell.EndianFormat.Big))
				{
					sout.Write(Content);
					sout.Seek(0);

					cs.InitializeFromStream(ms);
					cs.Compress();

					Write(s, EnumFlags.Test(Flags, FileFlags.EncryptContent), cs,
						userKey: Header.DataCryptKey, writeLeftovers: WriteLeftovers);
				}
			}
			else
				s.Write(Content);
		}
#endif
		#endregion
	};
}
