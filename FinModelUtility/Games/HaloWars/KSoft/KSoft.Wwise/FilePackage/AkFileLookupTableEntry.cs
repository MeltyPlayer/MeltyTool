using System;
using System.Runtime.InteropServices;

namespace KSoft.Wwise.FilePackage
{
	[StructLayout(LayoutKind.Explicit)]
	struct AkFileLookupTableEntry
		: IO.IEndianStreamSerializable
		, IComparable<AkFileLookupTableEntry>
	{
		/// <summary>Size of this struct on disk</summary>
		internal const uint kSizeOf = (sizeof(uint) * 2) + sizeof(ulong) + (sizeof(uint) * 2);

		[FieldOffset(0)] public uint FileId32;
		[FieldOffset(0)] public ulong FileId64;
		[FieldOffset(8)] public uint BlockSize;
		[FieldOffset(0x10)] public uint FileSize32;
		[FieldOffset(0x10)] public ulong FileSize64;
		[FieldOffset(0x18)] public uint StartingBlock;
		[FieldOffset(0x1C)] public uint LanguageId;

		public long FileOffset { get { return this.StartingBlock * this.BlockSize; } }

		#region IEndianStreamSerializable Members
		void SerializePre2011_2(IO.EndianStream s)
		{
			s.Stream(ref this.FileId32); s.Stream(ref this.BlockSize); s.Stream(ref this.FileSize64);
			s.Stream(ref this.StartingBlock); s.Stream(ref this.LanguageId);
		}
		void Serialize32(IO.EndianStream s)
		{
			s.Stream(ref this.FileId32); s.Stream(ref this.BlockSize); s.Stream(ref this.FileSize32);
			s.Stream(ref this.StartingBlock); s.Stream(ref this.LanguageId);
		}
		void Serialize64(IO.EndianStream s)
		{
			s.Stream(ref this.FileId64); s.Stream(ref this.BlockSize); s.Stream(ref this.FileSize32); 
			s.Stream(ref this.StartingBlock);
			s.Stream(ref this.LanguageId);
		}

		public void Serialize(IO.EndianStream s)
		{
			var settings = (s.Owner as AkFilePackage).Settings;

			if (AkVersion.HasWordSizeDependentLUT(settings.SdkVersion))
			{
				if (settings.Platform.ProcessorType.ProcessorSize == Shell.ProcessorSize.x32)
					this.Serialize32(s);
				else
					this.Serialize64(s);
			}
			else
				this.SerializePre2011_2(s);
		}
		#endregion

		#region IComparable<FileEntry> Members
		public int CompareTo(AkFileLookupTableEntry other)
		{
			if (this.FileId64 < other.FileId64) return -1;
			if (this.FileId64 > other.FileId64) return 1;
			if (this.LanguageId < other.LanguageId) return -1;
			if (this.LanguageId > other.LanguageId) return 1;

			return 0;
		}
		#endregion
	};
}