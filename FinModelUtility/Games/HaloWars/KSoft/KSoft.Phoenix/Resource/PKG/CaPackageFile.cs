using System;
using System.IO;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Resource.PKG
{
	public enum CaPackageVersion
		: ulong
	{
		Zero,
		NoAlignment,
		UsesAlignment,

		kNumberOf
	};

	public struct CaPackageEntry
		: IO.IEndianStreamSerializable
	{
		public const int kMaxNameLength = 511;

		// Technically a Pascal string with 64-bit length prefix, but I'm not adding 64-bit prefix support just for this one use case :|
		public string Name;
		public long Offset;
		public long Size;

		public int CalculateSerializedSize()
		{
			int size = 0;
			size += sizeof(long);
			size += this.Name.Length;
			size += sizeof(long); // Offset
			size += sizeof(long); // Size

			return size;
		}

		public void Serialize(IO.EndianStream s)
		{
			if (s.IsReading)
			{
				long position = s.Reader.BaseStream.Position;
				long name_length = s.Reader.ReadInt64();
				if (name_length < 0 || name_length > kMaxNameLength)
					throw new InvalidDataException("Invalid name length {0} at offset {1} in {2}".Format(name_length, position, s.StreamName));
				this.Name = s.Reader.ReadString(Memory.Strings.StringStorage.AsciiString, (int)name_length);
				this.Offset = s.Reader.ReadInt64();
				this.Size = s.Reader.ReadInt64();
			}
			else if (s.IsWriting)
			{
				s.Writer.Write((long) this.Name.Length);
				s.Writer.Write(this.Name, Memory.Strings.StringStorage.AsciiString);
				s.Writer.Write(this.Offset);
				s.Writer.Write(this.Size);
			}
		}
	};

	public class CaPackageFile
		: IO.IEndianStreamSerializable
	{
		public const string kFileExtension = ".pkg";

		[Memory.Strings.StringStorageMarkupAscii]
		public const string kSignature = "capack";
		public const ulong kCurrentVersion = (ulong)CaPackageVersion.UsesAlignment;
		public const int kDefaultAlignment = sizeof(long);
		public const int kMinFileEntryCount = 1;
		public const int kHeaderLength = 0
			+ 6 // kSignature.Length
			+ sizeof(CaPackageVersion)
			+ sizeof(long) // file entry count
			;

		public List<CaPackageEntry> FileEntries { get; private set; }
			= new List<CaPackageEntry>();

		public long Alignment = kDefaultAlignment;

		public bool HasEnoughFileEntries { get { return this.FileEntries.Count > kMinFileEntryCount; } }
		public bool UseAlignment { get { return kCurrentVersion >= (ulong)CaPackageVersion.UsesAlignment; } }

		public int CalculateHeaderAndFileChunksSize(CaPackageVersion version)
		{
			int size = 0;
			size += kHeaderLength;

			foreach (var entry in this.FileEntries)
			{
				size += entry.CalculateSerializedSize();
			}

			if (version >= CaPackageVersion.UsesAlignment)
			{
				size += sizeof(long); // Alignment
			}

			return size;
		}

		public void Serialize(IO.EndianStream s)
		{
			s.StreamSignature(kSignature, Memory.Strings.StringStorage.AsciiString);

			//s.StreamVersionEnum(ref Version, CaPackageVersion.kNumberOf);
			ulong version = kCurrentVersion;
			s.Stream(ref version);
			if (version <= 0 || version > kCurrentVersion)
				IO.VersionMismatchException.Assert(s.Reader, kCurrentVersion);

			this.SerializeAllocationTable(s);

			if (version >= (ulong)CaPackageVersion.UsesAlignment)
			{
				s.Stream(ref this.Alignment);
			}
		}

		void SerializeAllocationTable(IO.EndianStream s)
		{
			long entries_count = this.FileEntries.Count;
			s.Stream(ref entries_count);
			if (entries_count > 0)
			{
				if (s.IsReading)
				{
					this.FileEntries.Capacity = (int)entries_count;
					for (int x = 0; x < entries_count; x++)
					{
						var e = new CaPackageEntry();
						s.Stream(ref e);
						this.FileEntries.Add(e);
					}
				}
				else if (s.IsWriting)
				{
					foreach (var e in this.FileEntries)
					{
						var e_copy = e;
						s.Stream(ref e_copy);
					}
				}
			}

		}

		public byte[] ReadEntryBytes(IO.EndianStream s, CaPackageEntry entry)
		{
			Contract.Requires<ArgumentNullException>(s != null);

			if (entry.Offset < 0 || entry.Offset > s.BaseStream.Length)
			{
				throw new InvalidOperationException(string.Format(
					"File entry '{0}' offset @{1} is not within length #{2} of file {3}",
					entry.Name, entry.Offset, s.BaseStream.Length, s.StreamName));
			}

			long endOffset = entry.Offset + entry.Size;
			if (endOffset < 0 || endOffset > s.BaseStream.Length)
			{
				throw new InvalidOperationException(string.Format(
					"File entry '{0}' @{1} with size #{2} is not within length #{3} of file {4}",
					entry.Name, entry.Offset, entry.Size, s.BaseStream.Length, s.StreamName));
			}

			s.Seek(entry.Offset);
			byte[] bytes = new byte[entry.Size];
			s.Stream(bytes);

			return bytes;
		}

		public void WriteEntryBytes(IO.EndianStream s, ref CaPackageEntry entry, Stream entryStream)
		{
			Contract.Requires<ArgumentNullException>(s != null);
			Contract.Requires<ArgumentNullException>(entryStream != null);
			Contract.Assume(entry.Name.IsNotNullOrEmpty());
			Contract.Assume(entry.Offset == 0);
			Contract.Assume(entry.Size == 0);

			entry.Offset = s.BaseStream.Position;
			entry.Size = entryStream.Length;

			entryStream.CopyTo(s.BaseStream);
		}

		public void SetupHeaderAndEntries(CaPackageFileDefinition definition)
		{
			if (definition.Alignment != 0)
				this.Alignment = definition.Alignment;

			foreach (var file_name in definition.FileNames)
			{
				// #TODO
			}
		}

		public static bool VerifyIsPkg(IO.EndianReader s)
		{
			Contract.Requires<InvalidOperationException>(s.BaseStream.CanRead);
			Contract.Requires<InvalidOperationException>(s.BaseStream.CanSeek);

			var base_stream = s.BaseStream;
			if ((base_stream.Length - base_stream.Position) < kHeaderLength)
			{
				return false;
			}

			string sig = s.ReadString(Memory.Strings.StringStorage.AsciiString);
			ulong version = s.ReadUInt64();
			long file_entry_count = s.ReadInt64();

			base_stream.Seek(-kHeaderLength, SeekOrigin.Current);

			return sig == kSignature
				&& version < (ulong)CaPackageVersion.kNumberOf
				&& file_entry_count >= kMinFileEntryCount;
		}
	};
}

