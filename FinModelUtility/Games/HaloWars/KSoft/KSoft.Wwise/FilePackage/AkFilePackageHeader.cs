
namespace KSoft.Wwise.FilePackage
{
	struct AkFilePackageHeader
		: IO.IEndianStreamSerializable
	{
		const uint kSizeOfHeader = AkSubchunkHeader.kSizeOf + sizeof(int);

		static readonly Values.GroupTagData32 kSignature = new Values.GroupTagData32("AKPK", "audiokinetic_package");
		const uint kVersion = 1;

		public uint HeaderSize;

		public void InitializeSize(uint sdkVersion, uint langMapTotalSize)
		{
			this.HeaderSize = 0;
			this.HeaderSize += kSizeOfHeader;
			this.HeaderSize += sizeof(uint); // field for lang map size
			this.HeaderSize += sizeof(uint); // field for LUT size (sound banks)
			this.HeaderSize += sizeof(uint);    // field for LUT size (streamed files)
			if (AkVersion.HasExternalFiles(sdkVersion))
				this.HeaderSize += sizeof(uint); // field for LUT size (external files)
			this.HeaderSize += langMapTotalSize;
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamSignature(kSignature.TagString, Memory.Strings.StringStorage.AsciiString);
			s.Stream(ref this.HeaderSize);
			s.StreamVersion(kVersion);
		}
		#endregion
	};
}